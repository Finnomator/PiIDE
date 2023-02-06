using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace PiIDE.Wrapers {

    public class PythonREPL {

        public event DataReceivedEventHandler? DataReceived;

        private readonly Process process = new() {
            StartInfo = new ProcessStartInfo() {
                FileName = "python",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = true
        };

        private bool GotNewInput;
        private string? NewInput;

        public PythonREPL() {
            process.OutputDataReceived += (s, e) => {
                Debug.WriteLine("Got data");
                NewInput = e.Data;
                GotNewInput = true;
                DataReceived?.Invoke(s, e);
            };
            process.ErrorDataReceived += delegate {
                Debug.WriteLine("FJÖOSDIF");
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        public void WriteLine(string line) {
            process.StandardInput.WriteLine(line);
        }

        public void WriteLine(int line) {
            process.StandardInput.WriteLine(line);
        }

        public string? ReadLine() {
            while (!GotNewInput)
                Thread.Sleep(10);
            return NewInput;
        }
    }

    public static class JediWraper {

        public class Script {

            private const string WraperVariableName = "script";
            private readonly PythonREPL Repl = new();

            public Script(string? code = null, string? path = null) {

                Repl.DataReceived += Repl_DataReceived;

                string line = $"""
                    import jedi
                    import orjson

                    {WraperVariableName} = jedi.Script("{code}", path=r"{path}")
                    print("asdf", flush=True)
                    """;

                foreach (string l in line.Split("\r\n"))
                    Repl.WriteLine(l);
            }

            private void Repl_DataReceived(object sender, DataReceivedEventArgs e) {

            }

            public List<ReturnClasses.Completion> Complete(int line, int column, bool fuzzy = false) {
                Repl.WriteLine($"completions = {WraperVariableName}.complete({line}, {column}, fuzzy={(fuzzy ? 1 : 0)})");
                Repl.WriteLine("""
                    print(
                        orjson.dumps(
                            [
                                {
                                    "name": completion.name,
                                    "complete": completion.complete,
                                    "name_with_symbols": completion.name_with_symbols,
                                    "description": completion.description,
                                    "docstring": completion.docstring(),
                                    "is_keyword": completion.is_keyword,
                                    "module_name": completion.module_name,
                                    "module_path": str(completion.module_path),
                                    "type": completion.type,
                                    "line": completion.line,
                                    "column": completion.column,
                                    "type_hint": None,
                                }
                                for completion in completions
                            ]
                        ).decode("utf-8")
                    ,flush = True)
                    """);
                string? res = Repl.ReadLine();
                if (res is null)
                    return new();
                return JsonSerializer.Deserialize<List<ReturnClasses.Completion>>(res) ?? new();
            }

            /*
            public List<ReturnClasses.Name> Infer(int line, int column, bool onlyStubs = false, bool preferStubs = false) {

            }

            public List<ReturnClasses.Name> Goto(int line, int column, bool followImports = false, bool followBuiltinImports = false, bool onlyStubs = false, bool preferStubs = false) {

            }

            public IEnumerable<ReturnClasses.Name> Search(string str, bool allScopes = false) {

            }

            public IEnumerable<ReturnClasses.Completion> CompleteSearch(string str, Dictionary<object, object>? kwargs = null) {

            }

            public List<ReturnClasses.Name> Help(int line, int column) {

            }

            public List<ReturnClasses.Name> GetReferences(int line, int column, Dictionary<object, object>? kwargs = null) {

            }

            public List<ReturnClasses.Signature> GetSignatures(int line, int column) {

            }

            public ReturnClasses.Name GetContext(int line, int column) {

            }

            public List<ReturnClasses.Name> GetNames(Dictionary<object, object>? kwargs = null) {

            }
            */
        }

        public static class ReturnClasses {

            public abstract class BaseName {

                [JsonPropertyName("module_path")]
                public string ModulePath { get; set; }
                [JsonPropertyName("name")]
                public string Name { get; set; }
                [JsonPropertyName("type")]
                public string Type { get; set; }
                [JsonPropertyName("module_name")]
                public string ModuleName { get; set; }
                [JsonPropertyName("line")]
                public int Line { get; set; }
                [JsonPropertyName("column")]
                public int Column { get; set; }
                [JsonPropertyName("description")]
                public string Description { get; set; }
                [JsonPropertyName("full_name")]
                public string FullName { get; set; }

                /*
                public bool InBuiltinModule() {

                }

                public (int row, int column) GetDefinitionStartPosition() {

                }

                public (int row, int column) GetDefinitionEndPosition() {

                }

                public string Docstring(bool raw = false, bool fast = true) {

                }

                public bool IsStub() {

                }

                public bool IsSideEffect() {

                }

                public List<Name> Goto(bool followImports = false, bool followBuiltinImports = false, bool onlyStubs = false, bool preferStubs = false) {

                }

                public List<Name> Infer(bool onlyStubs = false, bool preferStubs = false) {

                }

                public Name Parent() {

                }

                public string GetLineCode(int before = 0, int after = 0) {

                }

                public List<BaseSignature> GetSignatures() {

                }

                public List<Name> Execute() {

                }

                public string GetTypeHint() {

                }
                */
            }

            public class Name : BaseName {
                /*
                public List<Name> DefinedNames() {

                }

                public bool IsDefinition() {

                }
                */
            }

            public class Completion : BaseName {
                [JsonPropertyName("complete")]
                public string Complete { get; set; }
                [JsonPropertyName("name_with_symbols")]
                public string NameWithSymbols { get; set; }

                /*
                public int GetCompletionPrefixLength() {

                }
                */
            }

            public class BaseSignature : Name {

                [JsonPropertyName("params")]
                public List<ParamName> Params { get; set; }
                /*
                public override string ToString() {

                }
                */
            }

            public class Signature : BaseSignature {
                public int Index { get; set; }
                public (int line, int column) BracketStart { get; set; }
            }

            public class ParamName : Name {

                /*
                public List<Name> InferDefault() {

                }

                public List<Name> InferAnnotation(Dictionary<object, object>? kwargs = null) {

                }

                public override string ToString() {

                }
                */
            }
        }
    }
}
