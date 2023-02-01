﻿using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PiIDE.Wrapers {
    internal static class PylintWraper {

        public const string PylintPath = "Assets/Lint/pylint.exe";

        public static async Task<PylintMessage[]> GetLintingAsync(string[] filePaths) {

            string args = $"--output-format=json --msg-template=\"{{path}}({{line}}): [{{msg_id}}{{obj}}] {{msg}}\" -j 0 \"{string.Join("\" \"", filePaths)}\"";

            Process pylintProcess = new() {
                StartInfo = new ProcessStartInfo() {
                    UseShellExecute = false,
                    FileName = PylintPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            pylintProcess.Start();
            string output = await pylintProcess.StandardOutput.ReadToEndAsync();
            pylintProcess.WaitForExit();
            try {
                return JsonSerializer.Deserialize<PylintMessage[]>(output);
            } catch {
#if DEBUG
                throw;
#else
                return System.Array.Empty<PylintMessage>();
#endif
            }
        }
    }

    public class PylintMessage {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";
        [JsonPropertyName("module")]
        public string Module { get; set; } = "";
        [JsonPropertyName("obj")]
        public string Obj { get; set; } = "";
        [JsonPropertyName("line")]
        public int Line { get; set; }
        [JsonPropertyName("column")]
        public int Column { get; set; }
        [JsonPropertyName("endLine")]
        public int? EndLine { get; set; }
        [JsonPropertyName("endColumn")]
        public int? EndColumn { get; set; }
        [JsonPropertyName("path")]
        public string Path { get; set; } = "";
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = "";
        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
        [JsonPropertyName("message-id")]
        public string MessageId { get; set; } = "";
    }
}