using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace PiIDE.Wrapers {
    internal static class JediSyntaxHighlighterWraper {

        private const string SyntaxHighlighterPath = "Assets/Jedi/syntax_highlighter.exe";
        private readonly static Process process = new() {
            StartInfo = new ProcessStartInfo() {
                FileName = SyntaxHighlighterPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
            }
        };

        static JediSyntaxHighlighterWraper() {
            process.Start();
        }

        public static JediName[] GetHighlightedWords(string filePath, string fileContent, bool enableTypeHints) {

            process.StandardInput.WriteLine(filePath);
            process.StandardInput.WriteLine(enableTypeHints? 1 : 0);
            process.StandardInput.WriteLine(Tools.CountLines(fileContent));
            process.StandardInput.WriteLine(fileContent);
            string? line = process.StandardOutput.ReadLine();

            if (line is null) {
#if DEBUG
                MessageBox.Show("The jedi language server failed to get syntax highlighting for this file", "Jedi Error", MessageBoxButton.OK, MessageBoxImage.Error);
# endif
                return Array.Empty<JediName>();
            }

            try {
                return JsonSerializer.Deserialize<JediName[]>(line) ?? Array.Empty<JediName>();
            } catch {
                return Array.Empty<JediName>();
            }
        }
    }

    public class JediName {

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
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
        public FontAwesome.WPF.FontAwesome? Icon { get; set; }


        [JsonPropertyName("line")]
        public int? Line { get; set; }
        [JsonPropertyName("column")]
        public int? Column { get; set; }
        [JsonPropertyName("type_hint")]
        public string? TypeHint { get; set; }
    }
}
