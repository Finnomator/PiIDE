using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PiIDE {
    internal static class JediCompletionWraper {
        public static readonly string CodeCompleterPath = "C:\\Users\\finnd\\source\\repos\\PiIDE\\PiIDE\\Assets\\Jedi\\code_completer.exe";
        private static readonly Process CompletionProcess;

        static JediCompletionWraper() {
            CompletionProcess = new Process() {
                StartInfo = new ProcessStartInfo() {
                    FileName = "cmd",
                    Arguments = $"/c {CodeCompleterPath}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                }
            };
            CompletionProcess.Start();
        }

        public static Completion[] GetCompletion(string filePath, int row, int col) {
            CompletionProcess.StandardInput.WriteLine(filePath);
            CompletionProcess.StandardInput.WriteLine(row.ToString());
            CompletionProcess.StandardInput.WriteLine(col.ToString());

            string? line = CompletionProcess.StandardOutput.ReadLine();

            if (line is null)
                throw new NullReferenceException();

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
        public int Line { get; set; }
    }
}
