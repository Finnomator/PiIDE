using System;
using System.Diagnostics;

namespace PiIDE.Wrappers;

public static class PythonWrapper {

    public static event DataReceivedEventHandler? PythonOutputDataReceived;
    public static event DataReceivedEventHandler? PythonErrorDataReceived;
    public static event EventHandler? PythonExited;

    private static readonly ProcessStartInfo PythonDefaultStartInfo = new() {
        FileName = "python",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        RedirectStandardInput = true,
        CreateNoWindow = true,
    };

    public static class AsyncFileRunner {

        private static Process? _process;

        public static async void RunFileAsync(string filePath) {
            _process = new() {
                StartInfo = PythonDefaultStartInfo,
                EnableRaisingEvents = true
            };
            _process.StartInfo.Arguments = $"\"{filePath}\"";

            _process.OutputDataReceived += (s, e) => PythonOutputDataReceived?.Invoke(s, e);
            _process.ErrorDataReceived += (s, e) => PythonErrorDataReceived?.Invoke(s, e);
            _process.Exited += (s, e) => PythonExited?.Invoke(s, e);

            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();

            await _process.WaitForExitAsync();
            _process.Close();
            _process = null;
        }

        public static void WriteLineToInput(string line) => _process?.StandardInput.WriteLine(line);

        public static void KillProcess() {
            if (_process == null)
                return;

            _process.Kill();
            _process.Close();
            _process = null;
        }
    }

    public static string RunFile(string filePath) {
        Process process = new() { StartInfo = PythonDefaultStartInfo };
        process.StartInfo.Arguments = filePath;

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        string? output = process.StandardOutput.ReadToEnd();
        string? error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        process.Close();

        if (output == null)
            return error;
        return output;
    }
}