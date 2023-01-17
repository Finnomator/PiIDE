using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static string GetCompletion(string filePath, int row, int col) {
            CompletionProcess.StandardInput.WriteLine(filePath);
            CompletionProcess.StandardInput.Flush();
            CompletionProcess.StandardInput.WriteLine(row.ToString());
            CompletionProcess.StandardInput.Flush();
            CompletionProcess.StandardInput.WriteLine(col.ToString());
            CompletionProcess.StandardInput.Flush();
            string? line = CompletionProcess.StandardOutput.ReadLine();

            if (line is null)
                throw new NullReferenceException();

            line = line.Substring(1);
            return line;
        }
    }
}
