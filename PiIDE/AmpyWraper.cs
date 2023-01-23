using System;
using System.Diagnostics;

namespace PiIDE {
    internal static class AmpyWraper {

        public static EventHandler? AmpyOutputDataReceived;

        private static readonly ProcessStartInfo AmpyDefaultStartInfo = new() {
            FileName = "ampy",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        static AmpyWraper() {

        }

        public static string ReadFileOnBoard(int comport, string filePath) {
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"get --port COM{comport} {filePath}";
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.StandardOutput.Close();
            return output;
        }

        public static void ReadFileOnBoardIntoFile(int comport, string filePath, string destPath) {
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"get --port COM{comport} {filePath} {destPath}";
            process.Start();
            process.WaitForExit();
            process.StandardOutput.Close();
        }

        public static void WriteToBoard(int comport, string fileOrDirPath, string destDir = "/") {
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"put --port COM{comport} {fileOrDirPath} {destDir}";
            process.Start();

            process.WaitForExit();
            process.StandardOutput.Close();
        }

        public static void CreateDirectory(int comport, string newDirPath) {
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"mkdir --port COM{comport} {newDirPath}";
            process.Start();

            process.WaitForExit();
            process.StandardOutput.Close();
        }

        public static string[] ListFilesOnBoard(int comport, string dirPath = "/") {

            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"ls --port COM{comport} {dirPath}";
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.StandardOutput.Close();

            return output.Split("\r\n");
        }

        public static void RunFileOnBoard(int comport, string filePath) {

            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"run --port COM{comport} {filePath}";
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (s, e) => AmpyOutputDataReceived?.Invoke(s, e);
            process.Start();
        }

        public static void RemoveFromBoard(int comport, string fileOrDirPath) {
            Process process = new() { StartInfo = AmpyDefaultStartInfo };
            process.StartInfo.Arguments = $"rm --port COM{comport} {fileOrDirPath}";
            process.Start();
            process.WaitForExit();
            process.StandardOutput.Close();
        }

        public static void Softreset(int comport) {
            Process process = new() { StartInfo = AmpyDefaultStartInfo };
            process.StartInfo.Arguments = $"reset --port COM{comport}";
            process.Start();
            process.WaitForExit();
            process.StandardOutput.Close();
        }
    }
}
