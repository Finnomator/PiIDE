﻿using System;
using System.Diagnostics;
using System.IO;

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

            process.StartInfo.Arguments = $"--port COM{comport} get \"{filePath}\"";
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();
            IsBusy = false;
            return output;
        }

        public static void ReadFileOnBoardIntoFile(int comport, string filePath, string destPath) {
            Debug.Assert(!IsBusy);

            IsBusy = true;

            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} get \"{filePath}\" \"{destPath}\"";
            process.Start();
            process.WaitForExit();
            process.Close();
            IsBusy = false;
        }

        public static void WriteToBoard(int comport, string fileOrDirPath, string destDir = "/") {
            Debug.Assert(!IsBusy);

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} put \"{fileOrDirPath}\" \"{destDir}\"";
            process.Start();

            process.WaitForExit();
            process.Close();
            IsBusy = false;
        }

        public static void CreateDirectory(int comport, string newDirPath) {
            Debug.Assert(!IsBusy);

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} mkdir \"{newDirPath}\"";
            process.Start();

            process.WaitForExit();
            process.Close();
            IsBusy = false;
        }

        public static string[] ListFilesOnBoard(int comport, string dirPath = "/") {
            Debug.Assert(!IsBusy);

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };

            process.StartInfo.Arguments = $"--port COM{comport} ls \"{dirPath}\"";
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
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
                Debug.Assert(!IsBusy);
                IsBusy = true;
                RunnerProcess = new() { StartInfo = AmpyDefaultStartInfo };
                RunnerProcess.StartInfo.Arguments = $"--port COM{comport} run \"{filePath}\"";
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

        public static void RemoveFromBoard(int comport, string fileOrDirPath) {
            Debug.Assert(!IsBusy);

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };
            process.StartInfo.Arguments = $"--port COM{comport} rm \"{fileOrDirPath}\"";
            process.Start();
            process.WaitForExit();
            process.Close();
            IsBusy = false;
        }

        public static void Softreset(int comport) {
            Debug.Assert(!IsBusy);

            IsBusy = true;
            Process process = new() { StartInfo = AmpyDefaultStartInfo };
            process.StartInfo.Arguments = $"--port COM{comport} reset";
            process.Start();
            process.WaitForExit();
            process.Close();
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
