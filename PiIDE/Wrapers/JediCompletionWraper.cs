using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PiIDE.Wrapers {
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

        public static async Task<Completion[]> GetCompletionAsync(string filePath, string fileContent, int row, int col) {

            if (!FinishedGettingCompletions)
                return Array.Empty<Completion>();

            FinishedGettingCompletions = false;

            CompletionProcess.StandardInput.WriteLine($"{filePath}");
            CompletionProcess.StandardInput.WriteLine(row);
            CompletionProcess.StandardInput.WriteLine(col);
            CompletionProcess.StandardInput.WriteLine(fileContent.Split('\n').Length);
            CompletionProcess.StandardInput.WriteLine(fileContent);

            string? line = await CompletionProcess.StandardOutput.ReadLineAsync();

            if (line is null) {
# if DEBUG
                throw new Exception(CompletionProcess.StandardError.ReadToEnd());
# else
                MessageBox.Show("The jedi language server failed to get completions for this file", "Jedi Error", MessageBoxButton.OK, MessageBoxImage.Error);
# endif
            }

            FinishedGettingCompletions = true;

            try {
                return JsonSerializer.Deserialize<Completion[]>(line);
            } catch {
#if DEBUG
                throw;
#else
                return Array.Empty<Completion>();
#endif
            }
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

        private string _Type;

        [JsonPropertyName("type")]
        public string Type {
            get { return _Type; }
            set {
                ForegroundColor = TypeColors.TypeToColor(value);
                _Type = value;
            }
        }
        public Brush ForegroundColor { get; set; }

        [JsonPropertyName("line")]
        public int? Line { get; set; }

    }
}
