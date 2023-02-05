using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Point = System.Drawing.Point;

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

        public static async Task<Completion[]> GetCompletionAsync(string filePath, string fileContent, bool enableTypeHints, Point colRow) {

            if (!FinishedGettingCompletions)
                return Array.Empty<Completion>();

            FinishedGettingCompletions = false;

            CompletionProcess.StandardInput.WriteLine(filePath);
            CompletionProcess.StandardInput.WriteLine(enableTypeHints ? 1 : 0);
            CompletionProcess.StandardInput.WriteLine(colRow.Y);
            CompletionProcess.StandardInput.WriteLine(colRow.X);
            CompletionProcess.StandardInput.WriteLine(Tools.CountLines(fileContent));
            CompletionProcess.StandardInput.WriteLine(fileContent);

            string? line = await CompletionProcess.StandardOutput.ReadLineAsync();

            if (line is null) {
# if DEBUG
                MessageBox.Show("The jedi language server failed to get completions for this file", "Jedi Error", MessageBoxButton.OK, MessageBoxImage.Error);
# endif
                return Array.Empty<Completion>();
            }


            FinishedGettingCompletions = true;

            try {
                return JsonSerializer.Deserialize<Completion[]>(line) ?? Array.Empty<Completion>();
            } catch {
                return Array.Empty<Completion>();
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

        private string _type = "";

        [JsonPropertyName("type")]
        public string Type {
            get { return _type; }
            set {
                ForegroundColor = TypeColors.TypeToColor(value);
                Icon = TypeIcons.TypeToIcon(value);
                _type = value;
            }
        }

        public Brush? ForegroundColor { get; private set; }
        public FontAwesome.WPF.FontAwesomeIcon Icon { get; private set; }

        [JsonPropertyName("line")]
        public int? Line { get; set; }
        [JsonPropertyName("column")]
        public int? Column { get; set; }
        [JsonPropertyName("type_hint")]
        public string? TypeHint { get; set; }
    }
}
