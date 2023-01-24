using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace PiIDE {
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

        static AmpyWraper() {

        }

        public static string ReadFileOnBoard(int comport, string filePath) {
            Debug.Assert(!IsBusy);
            IsBusy = true;

            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} get {filePath}";
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.StandardOutput.Close();
            IsBusy = false;
            return output;
        }

        public static void ReadFileOnBoardIntoFile(int comport, string filePath, string destPath) {
            Debug.Assert(!IsBusy);

            IsBusy = true;

            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} get {filePath} {destPath}";
            process.Start();
            process.WaitForExit();
            process.StandardOutput.Close();
            IsBusy = false;
        }

        public static void WriteToBoard(int comport, string fileOrDirPath, string destDir = "/") {
            Debug.Assert(!IsBusy);

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} put {fileOrDirPath} {destDir}";
            process.Start();

            process.WaitForExit();
            process.StandardOutput.Close();
            IsBusy = false;
        }

        public static void CreateDirectory(int comport, string newDirPath) {
            Debug.Assert(!IsBusy);

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} mkdir {newDirPath}";
            process.Start();

            process.WaitForExit();
            process.StandardOutput.Close();
            IsBusy = false;
        }

        public static string[] ListFilesOnBoard(int comport, string dirPath = "/") {
            Debug.Assert(!IsBusy);

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} ls {dirPath}";
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.StandardOutput.Close();
            IsBusy = false;

            try {
                output = output[1..];
            } catch (ArgumentOutOfRangeException) {
                return Array.Empty<string>();
            }

            return output.Trim().Split("\r\n/");
        }

        public static async Task RunFileOnBoardAsync(int comport, string filePath) {
            Debug.Assert(!IsBusy);
            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };
            process.StartInfo.Arguments = $"--port COM{comport} run {filePath}";
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (s, e) => AmpyOutputDataReceived?.Invoke(s, e);
            process.ErrorDataReceived += (s, e) => AmpyErrorDataReceived?.Invoke(s, e);
            process.Exited += (s, e) => AmpyExited?.Invoke(s, e);
            process.Start();
            await process.WaitForExitAsync();
            IsBusy = false;
        }

        public static void RemoveFromBoard(int comport, string fileOrDirPath) {
            Debug.Assert(!IsBusy);

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };
            process.StartInfo.Arguments = $"--port COM{comport} rm {fileOrDirPath}";
            process.Start();
            process.WaitForExit();
            process.StandardOutput.Close();
            IsBusy = false;
        }

        public static void Softreset(int comport) {
            Debug.Assert(!IsBusy);

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };
            process.StartInfo.Arguments = $"--port COM{comport} reset";
            process.Start();
            process.WaitForExit();
            process.StandardOutput.Close();
            IsBusy = false;
        }

        public static void DownloadDirectoryFromBoard(int comport, string dirPath, string destDirPath) {
            string[] subPaths = ListFilesOnBoard(comport, dirPath);

            for (int i = 0; i < subPaths.Length; ++i) {
                string subPath = subPaths[i];
                bool isDirectory = !Path.HasExtension(subPath);

                string fullDestPath = Path.Combine(destDirPath, subPath);

                if (isDirectory) {

                    if (!Directory.Exists(fullDestPath))
                        Directory.CreateDirectory(fullDestPath);

                    DownloadDirectoryFromBoard(comport, subPath, destDirPath);
                } else {

                    if (!File.Exists(fullDestPath))
                        File.Create(fullDestPath).Close();

                    ReadFileOnBoardIntoFile(comport, subPath, fullDestPath);
                }
            }
        }
    }
}
