using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PiIDE {
    internal static class JediCompletionWraper {

        public const string CodeCompleterPath = "Assets/Jedi/code_completer.exe";
        public static bool FinishedGettingCompletions { get; private set; } = true;

        private static readonly Process CompletionProcess = new() {
            StartInfo = new ProcessStartInfo() {
                FileName = CodeCompleterPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
            },
        };

        static JediCompletionWraper() {
            CompletionProcess.Start();
        }

        public static async Task<Completion[]> GetCompletionAsync(string filePath, int row, int col) {

            FinishedGettingCompletions = false;

            CompletionProcess.StandardInput.WriteLine(filePath);
            CompletionProcess.StandardInput.WriteLine(row);
            CompletionProcess.StandardInput.WriteLine(col);

            string line = await CompletionProcess.StandardOutput.ReadLineAsync();

            FinishedGettingCompletions = true;

            line = line[1..];

            return JsonSerializer.Deserialize<Completion[]>(line);
        }
    }

    public class Completion {

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("complete")]
        public string Complete { get; set; } = "";
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
        [JsonPropertyName("docstring")]
        public string Docstring { get; set; } = "";
        [JsonPropertyName("is_keyword")]
        public bool IsKeyword { get; set; }
        [JsonPropertyName("module_name")]
        public string ModuleName { get; set; } = "";
        [JsonPropertyName("module_path")]
        public string ModulePath { get; set; } = "";
        [JsonPropertyName("name_with_symbols")]
        public string NameWithSymbols { get; set; } = "";
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";
        [JsonPropertyName("line")]
        public int? Line { get; set; }
    }
}
