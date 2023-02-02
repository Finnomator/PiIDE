using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

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

        public static JediSyntaxHighlightedWord[] GetHighlightedWords(string filePath, string fileContent) {
            process.StandardInput.WriteLine(filePath);
            process.StandardInput.WriteLine(fileContent.Split('\n').Length);
            process.StandardInput.WriteLine(fileContent);
            string? line = process.StandardOutput.ReadLine();

            if (line is null) {
#if DEBUG
                MessageBox.Show("The jedi language server failed to get syntax highlighting for this file", "Jedi Error", MessageBoxButton.OK, MessageBoxImage.Error);
# endif
                return Array.Empty<JediSyntaxHighlightedWord>();
            }

            try {
                return JsonSerializer.Deserialize<JediSyntaxHighlightedWord[]>(line) ?? Array.Empty<JediSyntaxHighlightedWord>();
            } catch {
                return Array.Empty<JediSyntaxHighlightedWord>();
            }
        }
    }

    public class JediSyntaxHighlightedWord {

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("line")]
        public int Line { get; set; }
        [JsonPropertyName("column")]
        public int Column { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

    }
}
