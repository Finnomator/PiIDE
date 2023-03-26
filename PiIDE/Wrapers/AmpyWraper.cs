using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace PiIDE.Wrapers {
    internal static class AmpyWraper {

        public static DataReceivedEventHandler? AmpyOutputDataReceived;
        public static DataReceivedEventHandler? AmpyErrorDataReceived;
        public static EventHandler? AmpyExited;

        public static bool IsBusy { get; private set; }

        private static readonly ProcessStartInfo AmpyDefaultStartInfo = new() {
            FileName = "ampy",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true,
        };

        private static async Task<string?> TryRunAmpy(string arguments, bool expectsOutput) {

            if (IsBusy) {
                ErrorMessager.AmpyIsBusy();
                return null;
            }

            IsBusy = true;

            string? error = null;
            string? output = null;

            using (Process p = new() { StartInfo = AmpyDefaultStartInfo }) {
                p.StartInfo.Arguments = arguments;
                p.Start();

                if (expectsOutput)
                    output = await p.StandardOutput.ReadToEndAsync();

                error = await p.StandardError.ReadToEndAsync();

                await p.WaitForExitAsync();
            }

            if (!string.IsNullOrEmpty(error)) {
                MessageBox.Show(error, "Failed to run ampy", MessageBoxButton.OK, MessageBoxImage.Error);
                output = null;
            }

            IsBusy = false;

            return output;
        }

        public static async Task<string?> ReadFileOnBoardAsync(int comport, string filePath)
            => await TryRunAmpy($"--port COM{comport} get \"{filePath.Replace("\\", "/")}\"", true);

        public static async Task ReadFileOnBoardIntoFileAsync(int comport, string filePath, string destPath)
            => await TryRunAmpy($"--port COM{comport} get \"{filePath}\" \"{destPath.Replace("\\", "/")}\"", false);

        public static async Task WriteToBoardAsync(int comport, string fileOrDirPath, string destPath)
            => await TryRunAmpy($"--port COM{comport} put \"{fileOrDirPath}\" \"{destPath.Replace("\\", "/")}\"", false);

        public static async Task CreateDirectoryAsync(int comport, string newDirPath)
            => await TryRunAmpy($"--port COM{comport} mkdir \"{newDirPath.Replace("\\", "/")}\"", false);

        public static async Task<string[]?> ListFilesOnBoardAsync(int comport, string dirPath = "/") {
            string? output = await TryRunAmpy($"--port COM{comport} ls \"{dirPath.Replace("\\", "/")}\"", true);
            if (output == null || output.Length == 0)
                return null;
            return output[1..].Trim().Split("\r\n/");
        }

        public static class FileRunner {

            private static Process? RunnerProcess;

            public static async void RunFileOnBoardAsync(int comport, string filePath) {
                if (IsBusy) {
                    ErrorMessager.AmpyIsBusy();
                    return;
                }

                IsBusy = true;

                using (RunnerProcess = new() { StartInfo = AmpyDefaultStartInfo }) {
                    RunnerProcess.StartInfo.Arguments = $"--port COM{comport} run \"{filePath.Replace("\\", "/")}\"";
                    RunnerProcess.EnableRaisingEvents = true;
                    RunnerProcess.OutputDataReceived += (s, e) => AmpyOutputDataReceived?.Invoke(s, e);
                    RunnerProcess.ErrorDataReceived += (s, e) => AmpyErrorDataReceived?.Invoke(s, e);
                    RunnerProcess.Exited += (s, e) => AmpyExited?.Invoke(s, e);
                    RunnerProcess.Start();
                    RunnerProcess.BeginOutputReadLine();
                    RunnerProcess.BeginErrorReadLine();
                    await RunnerProcess.WaitForExitAsync();
                }

                RunnerProcess = null;
                IsBusy = false;
            }

            public static void WriteLineToRunningFileInput(string text) {
                if (IsBusy && RunnerProcess != null)
                    RunnerProcess.StandardInput.WriteLine(text);
            }

            public static void KillProcess() {
                if (RunnerProcess == null)
                    return;

                RunnerProcess.Kill();
                RunnerProcess.Close();
                RunnerProcess = null;
                IsBusy = false;
            }
        }

        public static async Task RemoveFileFromBoardAsync(int comport, string filePath)
            => await TryRunAmpy($"--port COM{comport} rm \"{filePath.Replace("\\", "/")}\"", false);

        public static async Task RemoveDirectoryFromBoardAsync(int comport, string dirPath)
            => await TryRunAmpy($"--port COM{comport} rmdir \"{dirPath.Replace("\\", "/")}\"", false);

        public static async Task Softreset(int comport) 
            => await TryRunAmpy($"--port COM{comport} reset", false);

        public static async Task DownloadDirectoryFromBoardAsync(int comport, string dirPath, string destDirPath) {

            if (IsBusy) {
                ErrorMessager.AmpyIsBusy();
                return;
            }

            string[]? subPaths = await ListFilesOnBoardAsync(comport, dirPath);

            if (subPaths == null)
                return;

            for (int i = 0; i < subPaths.Length; ++i) {
                string subPath = subPaths[i];
                bool isDirectory = !Path.HasExtension(subPath);

                string fullDestPath = Path.Combine(destDirPath, subPath);

                if (isDirectory) {

                    if (!Directory.Exists(fullDestPath))
                        Directory.CreateDirectory(fullDestPath);

                    await DownloadDirectoryFromBoardAsync(comport, subPath, destDirPath);
                } else {

                    if (!File.Exists(fullDestPath))
                        File.Create(fullDestPath).Close();

                    await ReadFileOnBoardIntoFileAsync(comport, subPath, fullDestPath);
                }
            }
        }
    }
}
