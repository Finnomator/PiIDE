using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PiIDE.Wrappers;

public class JediWrapper {

    public static class WrapperRepl {

        private class Wrapper {

            private Process WrapperProcess;

            private bool ReceivedOutputData;
            private string? NewOutputData;

            private readonly SemaphoreSlim SemaphoreSlim = new(1, 1);

            public bool IsBusy { get; private set; }

            public Wrapper() => InitProcess();

            private void InitProcess() {
                WrapperProcess = new() {
                    StartInfo = new ProcessStartInfo {
                        FileName = "Assets/Jedi/jedi_wraper.exe",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = true,
                    },
                    EnableRaisingEvents = true,
                };

                WrapperProcess.OutputDataReceived += (_, e) => {
                    NewOutputData = e.Data;
                    ReceivedOutputData = true;
                };

#if DEBUG
                WrapperProcess.ErrorDataReceived += (_, e) => {
                    Debug.WriteLine($"Error: {e.Data ?? "NULL"}");
                };
#endif

                WrapperProcess.Exited += (_, _) => {
#if DEBUG
                    MessageBox.Show("Jedi crashed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                    InitProcess();
                };

                WrapperProcess.Start();
                WrapperProcess.BeginErrorReadLine();
                WrapperProcess.BeginOutputReadLine();
            }

            public string? WriteLine(string line, bool expectsOutput) {

                IsBusy = true;

                WrapperProcess.StandardInput.WriteLine(line);

                if (expectsOutput) {
                    string? res = ReadOutput();
                    IsBusy = false;
                    return res;
                }

                IsBusy = false;
                return null;
            }

            private string? ReadOutput() {
                while (!ReceivedOutputData)
                    Thread.Sleep(10);
                ReceivedOutputData = false;
                return NewOutputData;
            }

            public async Task<string?> WriteLineAsync(string line, bool expectsOutput) {

                await SemaphoreSlim.WaitAsync();

                IsBusy = true;

                await WrapperProcess.StandardInput.WriteLineAsync(line);

                if (expectsOutput) {
                    string? res = await ReadOutputAsync();
                    SemaphoreSlim.Release();
                    IsBusy = false;
                    return res;
                }

                IsBusy = false;
                SemaphoreSlim.Release();
                return null;
            }

            private async Task<string?> ReadOutputAsync() {
                while (!ReceivedOutputData)
                    await Task.Delay(10);
                ReceivedOutputData = false;
                return NewOutputData;
            }
        }

        private const int AmountOfWrappers = 2;
        private static readonly Wrapper[] Wrappers;

        static WrapperRepl() {
            Wrappers = new Wrapper[AmountOfWrappers];
            for (int i = 0; i < AmountOfWrappers; i++)
                Wrappers[i] = new();
        }

        public static async Task<string?> WriteLineAsync(string line, bool expectsOutput) {
            Wrapper wrapper = AcquireAvailableWrapper();
            return await wrapper.WriteLineAsync(line, expectsOutput);
        }

        public static string? WriteLine(string line, bool expectsOutput) {
            Wrapper wrapper = AcquireAvailableWrapper();
            return wrapper.WriteLine(line, expectsOutput);
        }

        private static Wrapper AcquireAvailableWrapper() {
            for (int i = 0; i < AmountOfWrappers; i++) {
                if (!Wrappers[i].IsBusy)
                    return Wrappers[i];
            }
            return Wrappers[0];
        }
    }

    public class Script {

        public string Code { get; private set; }
        public string Path { get; private set; }

        public const string WrapperVariableName = "script";
        public const string CompletionsVarName = "completions";
        public const string NamesVarName = "names";

        private Script(string code, string path) {
            Code = code;
            Path = path;
        }

        public static async Task<Script> MakeScriptAsync(string code, string path) {
            await WrapperRepl.WriteLineAsync($"{WrapperVariableName} = jedi.Script(\"\"\"{code.Replace(@"\", @"\\").Replace("\r", @"\r").Replace("\n", @"\n").Replace("\"", "\\\"")}\"\"\", path=r\"{path}\")", false);
            return new(code, path);
        }

        public static Script MakeScript(string code, string path) {
            WrapperRepl.WriteLine($"{WrapperVariableName} = jedi.Script(\"\"\"{code.Replace(@"\", @"\\").Replace("\r", @"\r").Replace("\n", @"\n").Replace("\"", "\\\"")}\"\"\", path=r\"{path}\")", false);
            return new(code, path);
        }

        public static T[] TryConvert<T>(string? line) where T : ReturnClasses.BaseName {
            if (line == null)
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
            await WrapperRepl.WriteLineAsync($"{CompletionsVarName} = {WrapperVariableName}.complete({line}, {column}, fuzzy={(fuzzy ? 1 : 0)})", false);
            return TryConvert<ReturnClasses.Completion>(await WrapperRepl.WriteLineAsync("print_obj(dump_completions(completions))", true));
        }

        public async Task<ReturnClasses.Name[]> Infer(int line, int column, bool onlyStubs = false, bool preferStubs = false) {
            string? res = await WrapperRepl.WriteLineAsync($"print_obj(dump_names({WrapperVariableName}.infer({line}, {column}, only_stubs={(onlyStubs ? 1 : 0)}, prefer_stubs={(preferStubs ? 1 : 0)})))", true);
            return TryConvert<ReturnClasses.Name>(res);
        }

        public async Task<ReturnClasses.Name[]> Goto(int line, int column, bool followImports = false, bool followBuiltinImports = false, bool onlyStubs = false, bool preferStubs = false) {
            string? res = await WrapperRepl.WriteLineAsync($"print_obj(dump_names({WrapperVariableName}.goto({line}, {column}, follow_imports={(followImports ? 1 : 0)}, follow_builtin_imports={(followBuiltinImports ? 1 : 0)}, only_stubs={(onlyStubs ? 1 : 0)}, prefer_stubs={(preferStubs ? 1 : 0)})))", true);
            return TryConvert<ReturnClasses.Name>(res);
        }

        /*
        public async Task<IEnumerable<ReturnClasses.Name>> Search(string str, bool allScopes = false) {

        }

        public async Task<IEnumerable<ReturnClasses.Completion>> CompleteSearch(string str, Dictionary<object, object>? kwargs = null) {

        }
        */

        public async Task<ReturnClasses.Name[]> Help(int line, int column) => TryConvert<ReturnClasses.Name>(await WrapperRepl.WriteLineAsync($"print_obj(dump_names({WrapperVariableName}.help({line}, {column})))", true));

        public async Task<ReturnClasses.Name[]> GetReferences(int line, int column, Dictionary<object, object>? kwargs = null) {
            // TODO: implement kwargs
            kwargs ??= new();
            return TryConvert<ReturnClasses.Name>(await WrapperRepl.WriteLineAsync($"print_obj(dump_names({WrapperVariableName}.get_references({line}, {column})))", true));
        }

        public async Task<ReturnClasses.Signature[]> GetSignatures(int line, int column) => TryConvert<ReturnClasses.Signature>(await WrapperRepl.WriteLineAsync($"print_obj(dump_signatures({WrapperVariableName}.get_signatures({line} , {column})))", true));

        public async Task<ReturnClasses.Name?> GetContext(int line, int column) {
            string? res = await WrapperRepl.WriteLineAsync($"print_obj(dump_signatures({WrapperVariableName}.get_context({line}, {column})))", true);
            if (res == null)
                return null;
            return JsonSerializer.Deserialize<ReturnClasses.Name>(res);
        }

        public async Task<ReturnClasses.Name[]> GetNamesAsync(bool allScopes = false, bool definitions = false, bool references = false) {
            await WrapperRepl.WriteLineAsync($"{NamesVarName} = {WrapperVariableName}.get_names(all_scopes={(allScopes ? 1 : 0)}, definitions={(definitions ? 1 : 0)}, references={(references ? 1 : 0)})", false);
            return TryConvert<ReturnClasses.Name>(await WrapperRepl.WriteLineAsync("print_obj(dump_names(names))", true));
        }

        public ReturnClasses.Name[] GetNames(bool allScopes = false, bool definitions = false, bool references = false) {
            WrapperRepl.WriteLine($"{NamesVarName} = {WrapperVariableName}.get_names(all_scopes={(allScopes ? 1 : 0)}, definitions={(definitions ? 1 : 0)}, references={(references ? 1 : 0)})", false);
            return TryConvert<ReturnClasses.Name>(WrapperRepl.WriteLine("print_obj(dump_names(names))", true));
        }
    }

    public class ReturnClasses {

        public abstract class BaseName {

            [JsonPropertyName("module_path")]
            public required string? ModulePath { get; init; }
            [JsonPropertyName("name")]
            public required string Name { get; init; }

            private string PrivateType;
            [JsonPropertyName("type")]
            public required string Type {
                get => PrivateType; init {
                    PrivateType = value;
                    Foreground = ColorResources.HighlighterColors.GetBrush(value);
                    Icon = TypeIcons.TypeToIcon(value);
                }
            }

            [JsonPropertyName("module_name")]
            public required string ModuleName { get; init; }
            [JsonPropertyName("line")]
            public required int? Line { get; init; }
            [JsonPropertyName("column")]
            public required int? Column { get; init; }
            [JsonPropertyName("description")]
            public required string Description { get; init; }
            [JsonPropertyName("full_name")]
            public required string? FullName { get; init; }

            public Brush Foreground { get; init; }
            public FontAwesome.WPF.FontAwesome Icon { get; init; }

            public string VariableName { get; set; }
            public required Script Script;

            protected static T? TryConvert<T>(string? line) {
                if (line == null)
                    return default;
                return JsonSerializer.Deserialize<T>(line);
            }

            public async Task<bool> InBuiltinModule() => (await WrapperRepl.WriteLineAsync($"print_one_line({VariableName}.in_builtin_module())", true)) == "True";

            public async Task<(int row, int column)> GetDefinitionStartPosition() {
                string? res = await WrapperRepl.WriteLineAsync($"print_one_line({VariableName}.get_definition_start_position())", true);
                string[] parts = res.Split(", ");
                return (int.Parse(parts[0][1..]), int.Parse(parts[1][..1]));
            }

            public async Task<(int row, int column)> GetDefinitionEndPosition() {
                string? res = await WrapperRepl.WriteLineAsync($"print_one_line({VariableName}.get_definition_end_position())", true);
                string[] parts = res.Split(", ");
                return (int.Parse(parts[0][1..]), int.Parse(parts[1][..1]));
            }

            public async Task<string?> Docstring(bool raw = false, bool fast = true) {
                string? res = await WrapperRepl.WriteLineAsync($"print_one_line({VariableName}.docstring({(raw ? 1 : 0)}, {(fast ? 1 : 0)}))", true);
                return res?.Replace("\\n", "\n");
            }

            public async Task<bool> IsStub() => (await WrapperRepl.WriteLineAsync($"print_one_line({VariableName}.is_stub())", true)) == "True";

            public async Task<bool> IsSideEffect() => (await WrapperRepl.WriteLineAsync($"print_one_line({VariableName}.is_side_effect())", true)) == "True";

            public async Task<Name[]> Goto(bool followImports = false, bool followBuiltinImports = false, bool onlyStubs = false, bool preferStubs = false) {
                string? res = await WrapperRepl.WriteLineAsync($"print_obj(dump_names({VariableName}.goto(follow_imports={(followImports ? 1 : 0)}, follow_builtin_imports={(followBuiltinImports ? 1 : 0)}, only_stubs={(onlyStubs ? 1 : 0)}, prefer_stubs={(preferStubs ? 1 : 0)})))", true);
                return Script.TryConvert<Name>(res);
            }

            public async Task<Name[]> Infer(bool onlyStubs = false, bool preferStubs = false) {
                string? res = await WrapperRepl.WriteLineAsync($"print_obj(dump_names({VariableName}.infer(only_stubs={(onlyStubs ? 1 : 0)}, prefer_stubs={(preferStubs ? 1 : 0)})))", true);
                return Script.TryConvert<Name>(res);
            }

            public async Task<Name?> Parent() => TryConvert<Name>(await WrapperRepl.WriteLineAsync($"print_obj(dump_names({VariableName}.parent()))", true));

            public async Task<string?> GetLineCode(int before = 0, int after = 0) => await WrapperRepl.WriteLineAsync($"print_one_line({VariableName}.get_line_code({before}, {after}))", true);

            public async Task<BaseSignature[]> GetSignatures() => Script.TryConvert<BaseSignature>(await WrapperRepl.WriteLineAsync($"print_obj(dump_signatures({VariableName}.get_signatures()))", true));

            public async Task<Name[]> Execute() => Script.TryConvert<Name>(await WrapperRepl.WriteLineAsync($"print_obj(dump_names({VariableName}.execute()))", true));

            public async Task<string?> GetTypeHint() {
                string? res = await WrapperRepl.WriteLineAsync($"print_one_line({VariableName}.get_type_hint())", true);
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