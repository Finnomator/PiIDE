using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

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

        internal static Dictionary<string, Completion> GetCompletion(string filePath, int v1, double v2) {
            throw new NotImplementedException();
        }

        private static Dictionary<string, Completion> Convert(Dictionary<string, Dictionary<string, JsonElement>> data) {
            Dictionary<string, Completion> completions = new();

            foreach (string name in data.Keys) {

                Dictionary<string, JsonElement> value = data[name];

                int line;

                try {
                    line = value["line"].GetInt32();
                } catch (InvalidOperationException) {
                    line = -1;
                }

                completions[name] = new Completion(
                    name: name,
                    complete: value["complete"].ToString(),
                    description: value["description"].ToString(),
                    docstring: value["docstring"].ToString(),
                    isKeyword: value["is_keyword"].GetBoolean(),
                    moduleName: value["module_name"].ToString(),
                    modulePath: value["module_path"].ToString(),
                    nameWithSymbols: value["name_with_symbols"].ToString(),
                    type: value["type"].ToString(),
                    line: line
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
        public readonly string NameWithSymbols;
        public readonly string Type;
        public readonly int Line;

        public Completion(string name, string complete, string description, string docstring, bool isKeyword, string moduleName, string modulePath, string nameWithSymbols, string type, int line) {
            Name = name;
            Complete = complete;
            Description = description;
            Docstring = docstring;
            IsKeyword = isKeyword;
            ModuleName = moduleName;
            ModulePath = modulePath;
            NameWithSymbols = nameWithSymbols;
            Type = type;
            Line = line;
        }
    }

}
