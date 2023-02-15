using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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

        public static async Task<string> ReadFileOnBoardAsync(int comport, string filePath) {

            Debug.Assert(!IsBusy);

            IsBusy = true;

            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} get \"{filePath.Replace("\\", "/")}\"";
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            await process.WaitForExitAsync();
            process.Close();
            IsBusy = false;
            return output;
        }

        public static async Task ReadFileOnBoardIntoFileAsync(int comport, string filePath, string destPath) {
            if (IsBusy) {
                ErrorMessager.AmpyIsBusy();
                return;
            }

            IsBusy = true;

            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} get \"{filePath}\" \"{destPath.Replace("\\", "/")}\"";
            process.Start();
            await process.WaitForExitAsync();
            process.Close();
            IsBusy = false;
        }

        public static async Task WriteToBoardAsync(int comport, string fileOrDirPath, string destPath = "/") {
            if (IsBusy) {
                ErrorMessager.AmpyIsBusy();
                return;
            }

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} put \"{fileOrDirPath}\" \"{destPath.Replace("\\", "/")}\"";
            process.Start();

            await process.WaitForExitAsync();
            process.Close();
            IsBusy = false;
        }

        public static void CreateDirectory(int comport, string newDirPath) {
            if (IsBusy) {
                ErrorMessager.AmpyIsBusy();
                return;
            }
            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} mkdir \"{newDirPath.Replace("\\", "/")}\"";
            process.Start();

            process.WaitForExit();
            process.Close();
            IsBusy = false;
        }

        public static async Task<string[]> ListFilesOnBoardAsync(int comport, string dirPath = "/") {
            Debug.Assert(!IsBusy);

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} ls \"{dirPath.Replace("\\", "/")}\"";
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            await process.WaitForExitAsync();
            process.Close();
            IsBusy = false;

            try {
                output = output[1..];
            } catch (ArgumentOutOfRangeException) {
                return Array.Empty<string>();
            }

            return output.Trim().Split("\r\n/");
        }

        public static class FileRunner {

            private static Process? RunnerProcess;

            public static async void RunFileOnBoardAsync(int comport, string filePath) {
                if (IsBusy) {
                    ErrorMessager.AmpyIsBusy();
                    return;
                }

                IsBusy = true;
                RunnerProcess = new() { StartInfo = AmpyDefaultStartInfo };
                RunnerProcess.StartInfo.Arguments = $"--port COM{comport} run \"{filePath.Replace("\\", "/")}\"";
                RunnerProcess.EnableRaisingEvents = true;
                RunnerProcess.OutputDataReceived += (s, e) => AmpyOutputDataReceived?.Invoke(s, e);
                RunnerProcess.ErrorDataReceived += (s, e) => AmpyErrorDataReceived?.Invoke(s, e);
                RunnerProcess.Exited += (s, e) => AmpyExited?.Invoke(s, e);
                RunnerProcess.Start();
                RunnerProcess.BeginOutputReadLine();
                RunnerProcess.BeginErrorReadLine();
                await RunnerProcess.WaitForExitAsync();
                RunnerProcess.Close();
                RunnerProcess = null;
                IsBusy = false;
            }

            public static void WriteLineToRunningFileInput(string text) {
                if (IsBusy && RunnerProcess is not null)
                    RunnerProcess.StandardInput.WriteLine(text);
            }

            public static void KillProcess() {
                if (RunnerProcess is null)
                    return;

                RunnerProcess.Kill();
                RunnerProcess.Close();
                RunnerProcess = null;
                IsBusy = false;
            }
        }

        public static async Task RemoveFileFromBoardAsync(int comport, string filePath) {

            if (IsBusy) {
                ErrorMessager.AmpyIsBusy();
                return;
            }

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };
            process.StartInfo.Arguments = $"--port COM{comport} rm \"{filePath.Replace("\\", "/")}\"";
            process.Start();
            await process.WaitForExitAsync();
            process.Close();
            IsBusy = false;
        }

        public static async Task RemoveDirectoryFromBoardAsync(int comport, string dirPath) {

            if (IsBusy) {
                ErrorMessager.AmpyIsBusy();
                return;
            }

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };
            process.StartInfo.Arguments = $"--port COM{comport} rmdir \"{dirPath.Replace("\\", "/")}\"";
            process.Start();
            await process.WaitForExitAsync();
            process.Close();
            IsBusy = false;
        }

        public static void Softreset(int comport) {

            if (IsBusy) {
                ErrorMessager.AmpyIsBusy();
                return;
            }

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };
            process.StartInfo.Arguments = $"--port COM{comport} reset";
            process.Start();
            process.WaitForExit();
            process.Close();
            IsBusy = false;
        }

        public static async Task DownloadDirectoryFromBoardAsync(int comport, string dirPath, string destDirPath) {

            if (IsBusy) {
                ErrorMessager.AmpyIsBusy();
                return;
            }

            string[] subPaths = await ListFilesOnBoardAsync(comport, dirPath);

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
