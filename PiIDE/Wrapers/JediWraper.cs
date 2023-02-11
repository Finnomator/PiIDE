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

        public static class WraperRepl {
            private static readonly Process WraperProcess = new() {
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
            public static string WritenInput = "";
            public static string WritenOutput = "";
# endif

            private static bool ReceivedOutputData;
            private static string? NewOutputData;

            private static bool IsBusy;

            static WraperRepl() {

                WraperProcess.OutputDataReceived += (s, e) => {
#if DEBUG
                    WritenOutput += (e.Data ?? "NULL") + "\n";
                    Debug.WriteLine("Output: " + (e.Data ?? "NULL"));
#endif
                    NewOutputData = e.Data;
                    ReceivedOutputData = true;
                };

                WraperProcess.Exited += (s, e) => {
                    MessageBox.Show("Jedi crashed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                };

                WraperProcess.Start();
                WraperProcess.BeginErrorReadLine();
                WraperProcess.BeginOutputReadLine();
            }

            public static async Task<string?> WriteLine(string line, bool expectsOutput) {
# if DEBUG
                Debug.WriteLine("Input: " + line);
                WritenInput += line + "\n";
# endif
                while (IsBusy)
                    await Task.Delay(10);

                IsBusy = true;

                WraperProcess.StandardInput.WriteLine(line);

                if (expectsOutput) {
                    string? res = await ReadOutput();
                    IsBusy = false;
                    return res;
                }

                IsBusy = false;
                return null;
            }

            private static async Task<string?> ReadOutput() {
                while (!ReceivedOutputData) 
                    await Task.Delay(10);
                ReceivedOutputData = false;
                return NewOutputData;
            }
        }

        public class Script {

            public string Code { get; private set; }
            public string Path { get; private set; }

            public const string WraperVariableName = "script";
            public const string CompletionsVarName = "completions";
            public const string NamesVarName = "names";

            public Script(string code, string path) {
                // Repl should only be created once to avoid reinitializing the program (takes long)
                Code = code;
                Path = path;
                WraperRepl.WriteLine($"{WraperVariableName} = jedi.Script(\"\"\"{code.Replace(@"\", @"\\").Replace("\r\n", @"\r\n")}\"\"\", path=r\"{path}\")", false);
            }

            public static T[] TryConvert<T>(string? line) where T : ReturnClasses.BaseName {
                if (line is null)
                    return Array.Empty<T>();

                T[] x;

                try {
                    x = JsonSerializer.Deserialize<T[]>(line) ?? Array.Empty<T>();
                } catch {
                    return Array.Empty<T>();
                }

                for (int i = 0; i < x.Length; ++i) {
                    if (typeof(T) == typeof(ReturnClasses.Name))
                        x[i].VariableName = $"{NamesVarName}[{i}]";
                    else if (typeof(T) == typeof(ReturnClasses.Completion))
                        x[i].VariableName = $"{CompletionsVarName}[{i}]";
                }

                return x;
            }

            public async Task<ReturnClasses.Completion[]> Complete(int line, int column, bool fuzzy = false) {
                WraperRepl.WriteLine($"{CompletionsVarName} = {WraperVariableName}.complete({line}, {column}, fuzzy={(fuzzy ? 1 : 0)})", false);
                return TryConvert<ReturnClasses.Completion>(await WraperRepl.WriteLine($"print_obj(dump_completions(completions))", true));
            }

            public async Task<ReturnClasses.Name[]> Infer(int line, int column, bool onlyStubs = false, bool preferStubs = false) {
                string? res = await WraperRepl.WriteLine($"print_obj(dump_names({WraperVariableName}.infer({line}, {column}, only_stubs={(onlyStubs ? 1 : 0)}, prefer_stubs={(preferStubs ? 1 : 0)})))", true);
                return TryConvert<ReturnClasses.Name>(res);
            }

            public async Task<ReturnClasses.Name[]> Goto(int line, int column, bool followImports = false, bool followBuiltinImports = false, bool onlyStubs = false, bool preferStubs = false) {
                string? res = await WraperRepl.WriteLine($"print_obj(dump_names({WraperVariableName}.goto({line}, {column}, follow_imports={(followImports ? 1 : 0)}, follow_builtin_imports={(followBuiltinImports ? 1 : 0)}, only_stubs={(onlyStubs ? 1 : 0)}, prefer_stubs={(preferStubs ? 1 : 0)})))", true);
                return TryConvert<ReturnClasses.Name>(res);
            }

            /*
            public async Task<IEnumerable<ReturnClasses.Name>> Search(string str, bool allScopes = false) {

            }

            public async Task<IEnumerable<ReturnClasses.Completion>> CompleteSearch(string str, Dictionary<object, object>? kwargs = null) {

            }
            */

            public async Task<ReturnClasses.Name[]> Help(int line, int column) {
                return TryConvert<ReturnClasses.Name>(await WraperRepl.WriteLine($"print_obj(dump_names({WraperVariableName}.help({line}, {column})))", true));
            }

            public async Task<ReturnClasses.Name[]> GetReferences(int line, int column, Dictionary<object, object>? kwargs = null) {
                // TODO: implement kwargs
                kwargs ??= new();
                return TryConvert<ReturnClasses.Name>(await WraperRepl.WriteLine($"print_obj(dump_names({WraperVariableName}.get_references({line}, {column})))", true));
            }

            public async Task<ReturnClasses.Signature[]> GetSignatures(int line, int column) {
                return TryConvert<ReturnClasses.Signature>(await WraperRepl.WriteLine($"print_obj(dump_signatures({WraperVariableName}.get_signatures({line} , {column})))", true));
            }

            public async Task<ReturnClasses.Name?> GetContext(int line, int column) {
                string? res = await WraperRepl.WriteLine($"print_obj(dump_signatures({WraperVariableName}.get_context({line}, {column})))", true);
                if (res is null)
                    return null;
                return JsonSerializer.Deserialize<ReturnClasses.Name>(res);
            }

            public async Task<ReturnClasses.Name[]> GetNames(bool allScopes = false, bool definitions = false, bool references = false) {
                WraperRepl.WriteLine($"{NamesVarName} = {WraperVariableName}.get_names(all_scopes={(allScopes ? 1 : 0)}, definitions={(definitions ? 1 : 0)}, references={(references ? 1 : 0)})", false);
                return TryConvert<ReturnClasses.Name>(await WraperRepl.WriteLine($"print_obj(dump_names(names))", true));
            }
        }

        public class ReturnClasses {

            public abstract class BaseName {

                [JsonPropertyName("module_path")]
                public required string? ModulePath { get; set; }
                [JsonPropertyName("name")]
                public required string Name { get; set; }

                public string _type;
                [JsonPropertyName("type")]
                public required string Type {
                    get => _type; set {
                        _type = value;
                        Foreground = TypeColors.TypeToColor(value);
                        Icon = TypeIcons.TypeToIcon(value);
                    }
                }

                [JsonPropertyName("module_name")]
                public required string ModuleName { get; set; }
                [JsonPropertyName("line")]
                public required int? Line { get; set; }
                [JsonPropertyName("column")]
                public required int? Column { get; set; }
                [JsonPropertyName("description")]
                public required string Description { get; set; }
                [JsonPropertyName("full_name")]
                public required string? FullName { get; set; }

                public Brush Foreground { get; set; }
                public FontAwesome.WPF.FontAwesome Icon { get; set; }

                public required string VariableName { get; set; }
                public required Script script;

                public BaseName() { }

                protected static T? TryConvert<T>(string? line) {
                    if (line is null)
                        return default;
                    return JsonSerializer.Deserialize<T>(line);
                }

                public async Task<bool> InBuiltinModule() {
                    return (await WraperRepl.WriteLine($"print_one_line({VariableName}.in_builtin_module())", true)) == "True";
                }

                public async Task<(int row, int column)> GetDefinitionStartPosition() {
                    string? res = await WraperRepl.WriteLine($"print_one_line({VariableName}.get_definition_start_position())", true);
                    string[] parts = res.Split(", ");
                    return (int.Parse(parts[0][1..]), int.Parse(parts[1][..1]));
                }

                public async Task<(int row, int column)> GetDefinitionEndPosition() {
                    ;
                    string? res = await WraperRepl.WriteLine($"print_one_line({VariableName}.get_definition_end_position())", true);
                    string[] parts = res.Split(", ");
                    return (int.Parse(parts[0][1..]), int.Parse(parts[1][..1]));
                }

                public async Task<string?> Docstring(bool raw = false, bool fast = true) {
                    string? res = await WraperRepl.WriteLine($"print_one_line({VariableName}.docstring({(raw ? 1 : 0)}, {(fast ? 1 : 0)}))", true);
                    return res?.Replace("\\n", "\n");
                }

                public async Task<bool> IsStub() {
                    return (await WraperRepl.WriteLine($"print_one_line({VariableName}.is_stub())", true)) == "True";
                }

                public async Task<bool> IsSideEffect() {
                    return (await WraperRepl.WriteLine($"print_one_line({VariableName}.is_side_effect())", true)) == "True";
                }

                public async Task<Name[]> Goto(bool followImports = false, bool followBuiltinImports = false, bool onlyStubs = false, bool preferStubs = false) {
                    string? res = await WraperRepl.WriteLine($"print_obj(dump_names({VariableName}.goto(follow_imports={(followImports ? 1 : 0)}, follow_builtin_imports={(followBuiltinImports ? 1 : 0)}, only_stubs={(onlyStubs ? 1 : 0)}, prefer_stubs={(preferStubs ? 1 : 0)})))", true);
                    return Script.TryConvert<Name>(res);
                }

                public async Task<Name[]> Infer(bool onlyStubs = false, bool preferStubs = false) {
                    string? res = await WraperRepl.WriteLine($"print_obj(dump_names({VariableName}.infer(only_stubs={(onlyStubs ? 1 : 0)}, prefer_stubs={(preferStubs ? 1 : 0)})))", true);
                    return Script.TryConvert<Name>(res);
                }

                public async Task<Name?> Parent() {
                    return TryConvert<Name>(await WraperRepl.WriteLine($"print_obj(dump_names({VariableName}.parent()))", true));
                }

                public async Task<string?> GetLineCode(int before = 0, int after = 0) {
                    return await WraperRepl.WriteLine($"print_one_line({VariableName}.get_line_code({before}, {after}))", true);
                }

                public async Task<BaseSignature[]> GetSignatures() {
                    return Script.TryConvert<BaseSignature>(await WraperRepl.WriteLine($"print_obj(dump_signatures({VariableName}.get_signatures()))", true));
                }

                public async Task<Name[]> Execute() {
                    return Script.TryConvert<Name>(await WraperRepl.WriteLine($"print_obj(dump_names({VariableName}.execute()))", true));
                }

                public async Task<string?> GetTypeHint() {
                    string? res = await WraperRepl.WriteLine($"print_one_line({VariableName}.get_type_hint())", true);
                    return res == "None" ? null : res;
                }
            }

            public class Name : BaseName {

                /*
                public Name[] DefinedNames() {

                }

                public bool IsDefinition() {

                }
                */
            }

            public class Completion : BaseName {
                [JsonPropertyName("complete")]
                public required string? Complete { get; set; }
                [JsonPropertyName("name_with_symbols")]
                public required string NameWithSymbols { get; set; }

                /*
                public int GetCompletionPrefixLength() {

                }
                */
            }

            public class BaseSignature : Name {

                [JsonPropertyName("params")]
                public required ParamName[] Params { get; set; }

                /*
                public override string ToString() {

                }
                */
            }

            public class Signature : BaseSignature {
                public required int Index { get; set; }
                public required (int line, int column) BracketStart { get; set; }
            }

            public class ParamName : Name {
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
