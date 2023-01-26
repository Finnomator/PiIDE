using System;
using System.Diagnostics;

namespace PiIDE {
    public static class PythonWraper {

        public static DataReceivedEventHandler? PythonOutputDataReceived;
        public static DataReceivedEventHandler? PythonErrorDataReceived;
        public static EventHandler? PythonExited;

        private static readonly ProcessStartInfo PythonDefaultStartInfo = new() {
            FileName = "python",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true,
        };

        static PythonWraper() {

        }

        public static class AsyncFileRunner {

            private static Process? process;

            public static async void RunFileAsync(string filePath) {
                process = new() {
                    StartInfo = PythonDefaultStartInfo,
                    EnableRaisingEvents = true
                };
                process.StartInfo.Arguments = filePath;

                process.OutputDataReceived += (s, e) => PythonOutputDataReceived?.Invoke(s, e);
                process.ErrorDataReceived += (s, e) => PythonErrorDataReceived?.Invoke(s, e);
                process.Exited += (s, e) => PythonExited?.Invoke(s, e);

                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                await process.WaitForExitAsync();
                process.Close();
                process = null;
            }

            public static void WriteLineToInput(string line) {
                process?.StandardInput.WriteLine(line);
            }

            public static void KillProcess() {
                if (process is null)
                    return;

                process.Kill();
                process.Close();
                process = null;
            }
        }

        public static string RunFile(string filePath) {
            Process process = new() { StartInfo = PythonDefaultStartInfo };
            process.StartInfo.Arguments = filePath;

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            process.Close();

            if (output is null)
                return error;
            return output;
        }
    }
}
