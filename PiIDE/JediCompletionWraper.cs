using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows.Shapes;

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
            CompletionProcess.StandardInput.AutoFlush = true;

        }

        public static Dictionary<string, Completion> GetCompletion(string filePath, int row, int col) {
            CompletionProcess.StandardInput.WriteLine(filePath);
            CompletionProcess.StandardInput.WriteLine(row.ToString());
            CompletionProcess.StandardInput.WriteLine(col.ToString());

            string? line = CompletionProcess.StandardOutput.ReadLine();

            if (line is null)
                throw new NullReferenceException();

            line = line[1..];

            var deserialized = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(line);

            if (deserialized is null)
                throw new NullReferenceException();

            return Convert(deserialized);
        }

        private static Dictionary<string, Completion> Convert(Dictionary<string, Dictionary<string, JsonElement>> data) {
            Dictionary<string, Completion> completions = new();

            foreach (string name in data.Keys) {

                Dictionary<string, JsonElement> value = data[name];

                completions[name] = new Completion(
                    name,
                    value["complete"].ToString(),
                    value["description"].ToString(),
                    value["docstring"].ToString(),
                    value["is_keyword"].GetBoolean(),
                    value["module_name"].ToString(),
                    value["module_path"].ToString()
                );
            }

            return completions;
        }
    }

    public class Completion {

        public readonly string Name;
        public readonly string Complete;
        public readonly string Description;
        public readonly string Docstring;
        public readonly bool IsKeyword;
        public readonly string ModuleName;
        public readonly string ModulePath;

        public Completion(string name, string complete, string description, string docstring, bool isKeyword, string moduleName, string modulePath) {
            Name = name;
            Complete = complete;
            Description = description;
            Docstring = docstring;
            IsKeyword = isKeyword;
            ModuleName = moduleName;
            ModulePath = modulePath;
        }
    }

}
