using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PiIDE.Wrapers {

    public class JediWraper {

        public class WraperRepl {
            private readonly Process WraperProcess = new() {
                StartInfo = new ProcessStartInfo() {
                    FileName = "Assets/Jedi/jedi_wraper.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true,
            };

#if DEBUG
            public string WritenInput = "";
# endif

            private bool ReceivedOutputData;
            private string? NewOutputData;

            private bool ReceivedErrorData;
            private string? NewErrorData;

            public WraperRepl() {

                WraperProcess.OutputDataReceived += (s, e) => {
                    NewOutputData = e.Data;
                    ReceivedOutputData = true;
                };

                WraperProcess.ErrorDataReceived += (s, e) => {
                    NewErrorData = e.Data;
                    ReceivedErrorData = true;
                };

                WraperProcess.Exited += (s, e) => {
                    MessageBox.Show("Jedi crashed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    string inp = WritenInput;
                };

                WraperProcess.Start();
                WraperProcess.BeginErrorReadLine();
                WraperProcess.BeginOutputReadLine();
            }

            public void WriteLine(string line) {
# if DEBUG
                WritenInput += line + "\n";
# endif
                WraperProcess.StandardInput.WriteLine(line);
            }

            public async Task<string?> ReadOutput() {
                while (!ReceivedOutputData)
                    await Task.Delay(10);
                ReceivedOutputData = false;
                return NewOutputData;
            }

            public string? ReadError() {
                while (!ReceivedErrorData)
                    Thread.Sleep(10);
                ReceivedErrorData = false;
                return NewErrorData;
            }
        }

        public class Script {

            public const string WraperVariableName = "script";
            public readonly WraperRepl Repl;
            public string Code { get; private set; }
            public string Path { get; private set; }

            public Script(WraperRepl repl,  string code, string path) {
                // Repl should only be created once to avoid reinitializing the program (takes long)
                Code = code;
                Path = path;
                Repl = repl;
                Repl.WriteLine($"{WraperVariableName} = jedi.Script(\"\"\"{code.Replace("\r\n", "\\r\\n")}\"\"\", path=r\"{path}\")");
            }

            public Script() { }

            public T[] TryConvert<T>(string? line) where T : ReturnClasses.BaseName {
                if (line is null)
                    return Array.Empty<T>();
                T[] x = JsonSerializer.Deserialize<T[]>(line) ?? Array.Empty<T>();

                for (int i = 0; i < x.Length; ++i)
                    x[i].Repl = Repl;

                return x;
            }

            public async Task<ReturnClasses.Completion[]> Complete(int line, int column, bool fuzzy = false) {
                Repl.WriteLine($"print_obj(dump_completions({WraperVariableName}.complete({line}, {column}, fuzzy={(fuzzy ? 1 : 0)})))");
                return TryConvert<ReturnClasses.Completion>(await Repl.ReadOutput());
            }

            public async Task<ReturnClasses.Name[]> Infer(int line, int column, bool onlyStubs = false, bool preferStubs = false) {
                Repl.WriteLine($"print_obj(dump_names({WraperVariableName}.infer({line}, {column}, only_stubs={(onlyStubs ? 1 : 0)}, prefer_stubs={(preferStubs ? 1 : 0)})))");
                return TryConvert<ReturnClasses.Name>(await Repl.ReadOutput());
            }

            public async Task<ReturnClasses.Name[]> Goto(int line, int column, bool followImports = false, bool followBuiltinImports = false, bool onlyStubs = false, bool preferStubs = false) {
                Repl.WriteLine($"print_obj(dump_names({WraperVariableName}.goto({line}, {column}, follow_imports={(followImports ? 1 : 0)}, follow_builtin_imports={(followBuiltinImports ? 1 : 0)}, only_stubs={(onlyStubs ? 1 : 0)}, prefer_stubs={(preferStubs ? 1 : 0)})))");
                return TryConvert<ReturnClasses.Name>(await Repl.ReadOutput());
            }

            /*
            public async Task<IEnumerable<ReturnClasses.Name>> Search(string str, bool allScopes = false) {

            }

            public async Task<IEnumerable<ReturnClasses.Completion>> CompleteSearch(string str, Dictionary<object, object>? kwargs = null) {

            }
            */

            public async Task<ReturnClasses.Name[]> Help(int line, int column) {
                Repl.WriteLine($"print_obj(dump_names({WraperVariableName}.help({line}, {column})))");
                return TryConvert<ReturnClasses.Name>(await Repl.ReadOutput());
            }

            public async Task<ReturnClasses.Name[]> GetReferences(int line, int column, Dictionary<object, object>? kwargs = null) {
                // TODO: implement kwargs
                kwargs ??= new();
                Repl.WriteLine($"print_obj(dump_names({WraperVariableName}.get_references({line}, {column})))");
                return TryConvert<ReturnClasses.Name>(await Repl.ReadOutput());
            }

            public async Task<ReturnClasses.Signature[]> GetSignatures(int line, int column) {
                Repl.WriteLine($"print_obj(dump_signatures({WraperVariableName}.get_signatures({line} , {column})))");
                return TryConvert<ReturnClasses.Signature>(await Repl.ReadOutput());
            }

            public async Task<ReturnClasses.Name?> GetContext(int line, int column) {
                Repl.WriteLine($"print_obj(dump_signatures({WraperVariableName}.get_context({line}, {column})))");
                string? res = await Repl.ReadOutput();
                if (res is null)
                    return null;
                return JsonSerializer.Deserialize<ReturnClasses.Name>(res);
            }

            public async Task<ReturnClasses.Name[]> GetNames(bool allScopes = false, bool definitions = false, bool references = false) {
                Repl.WriteLine($"print_obj(dump_names({WraperVariableName}.get_names(all_scopes={(allScopes ? 1 : 0)}, definitions={(definitions ? 1 : 0)}, references={(references ? 1 : 0)})))");
                return TryConvert<ReturnClasses.Name>(await Repl.ReadOutput());
            }

        }

        public class ReturnClasses {

            public abstract class BaseName {

                [JsonPropertyName("module_path")]
                public string ModulePath { get; set; }
                [JsonPropertyName("name")]
                public string? Name { get; set; }

                public string _type;
                [JsonPropertyName("type")]
                public string Type {
                    get => _type; set {
                        _type = value;
                        Foreground = TypeColors.TypeToColor(value);
                        Icon = TypeIcons.TypeToIcon(value);
                    }
                }

                [JsonPropertyName("module_name")]
                public string ModuleName { get; set; }
                [JsonPropertyName("line")]
                public int? Line { get; set; }
                [JsonPropertyName("column")]
                public int? Column { get; set; }
                [JsonPropertyName("description")]
                public string Description { get; set; }
                [JsonPropertyName("full_name")]
                public string? FullName { get; set; }

                public Brush Foreground { get; set; }
                public FontAwesome.WPF.FontAwesome Icon { get; set; }

                protected readonly string VariableName = "base_name";
                public WraperRepl Repl { get; set; }
                protected readonly Script script;

                public BaseName(Script script) {
                    this.script = script;
                    Repl = script.Repl;
                }

                public BaseName() { }

                protected static T? TryConvert<T>(string? line) {
                    if (line is null)
                        return default;
                    return JsonSerializer.Deserialize<T>(line);
                }

                public async Task<bool> InBuiltinModule() {
                    Repl.WriteLine($"print({VariableName}.in_builtin_module(), flush=True)");
                    return TryConvert<bool>(await Repl.ReadOutput());
                }

                public async Task<(int row, int column)> GetDefinitionStartPosition() {
                    Repl.WriteLine($"print({VariableName}.get_definition_start_position(), flush=True)");
                    return TryConvert<(int, int)>(await Repl.ReadOutput());
                }

                public async Task<(int row, int column)> GetDefinitionEndPosition() {
                    Repl.WriteLine($"print({VariableName}.get_definition_end_position(), flush=True)");
                    return TryConvert<(int, int)>(await Repl.ReadOutput());
                }

                public async Task<string?> Docstring(bool raw = false, bool fast = true) {
                    Repl.WriteLine($"print({VariableName}.docstring({(raw ? 1 : 0)}, {(fast ? 1 : 0)}), flush=True)");
                    return TryConvert<string>(await Repl.ReadOutput());
                }

                public async Task<bool> IsStub() {
                    Repl.WriteLine($"print({VariableName}.is_stub(), flush=True)");
                    return TryConvert<bool>(await Repl.ReadOutput());
                }

                public async Task<bool> IsSideEffect() {
                    Repl.WriteLine($"print({VariableName}.is_side_effect(), flush=True)");
                    return TryConvert<bool>(await Repl.ReadOutput());
                }

                public async Task<Name[]> Goto(bool followImports = false, bool followBuiltinImports = false, bool onlyStubs = false, bool preferStubs = false) {
                    Repl.WriteLine($"print({VariableName}.goto(follow_imports={(followImports ? 1 : 0)}, follow_builtin_imports={(followBuiltinImports ? 1 : 0)}, only_stubs={(onlyStubs ? 1 : 0)}, prefer_stubs={(preferStubs ? 1 : 0)}), flush=True)");
                    return script.TryConvert<Name>(await Repl.ReadOutput());
                }

                public async Task<Name[]> Infer(bool onlyStubs = false, bool preferStubs = false) {
                    Repl.WriteLine($"print({VariableName}.infer(only_stubs={(onlyStubs ? 1 : 0)}, prefer_stubs={(preferStubs ? 1 : 0)}), flush=True)");
                    return script.TryConvert<Name>(await Repl.ReadOutput());
                }

                public async Task<Name?> Parent() {
                    Repl.WriteLine($"print({VariableName}.parent(), flush=True)");
                    return TryConvert<Name>(await Repl.ReadOutput());
                }

                public async Task<string?> GetLineCode(int before = 0, int after = 0) {
                    Repl.WriteLine($"print({VariableName}.get_line_code({before}, {after}), flush=True)");
                    return TryConvert<string>(await Repl.ReadOutput());
                }

                public async Task<BaseSignature[]> GetSignatures() {
                    Repl.WriteLine($"print({VariableName}.get_signatures(), flush=True)");
                    return script.TryConvert<BaseSignature>(await Repl.ReadOutput());
                }

                public async Task<Name[]> Execute() {
                    Repl.WriteLine($"print({VariableName}.execute(), flush=True)");
                    return script.TryConvert<Name>(await Repl.ReadOutput());
                }

                public async Task<string?> GetTypeHint() {
                    Repl.WriteLine($"print({VariableName}.get_type_hint(), flush=True)");
                    return TryConvert<string>(await Repl.ReadOutput());
                }
            }

            public class Name : BaseName {

                public Name(Script script) : base(script) {

                }

                public Name() { }

                /*
                public Name[] DefinedNames() {

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

                public Completion(Script script) : base(script) {

                }

                public Completion() { }

                /*
                public int GetCompletionPrefixLength() {

                }
                */
            }

            public class BaseSignature : Name {

                [JsonPropertyName("params")]
                public List<ParamName> Params { get; set; }

                public BaseSignature(Script script) : base(script) {

                }

                public BaseSignature() { }

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

                public ParamName(Script script) : base(script) {

                }

                public ParamName() { }

                /*
                public Name[] InferDefault() {

                }

                public Name[] InferAnnotation(Dictionary<object, object>? kwargs = null) {

                }

                public override string ToString() {

                }
                */
            }
        }
    }
}
