﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace PiIDE.Wrappers;

internal static class AmpyWrapper {

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

    private static async Task<(bool success, string? output)> TryRunAmpy(string arguments, bool expectsOutput) {

        if (IsBusy) {
            ErrorMessages.AmpyIsBusy();
            return (false, null);
        }

        IsBusy = true;

        string? error;
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
            IsBusy = false;
            return (false, null);
        }

        IsBusy = false;

        return (true, output);
    }

    public static async Task<(bool success, string? output)> ReadFileOnBoardAsync(int comport, string filePath)
        => await TryRunAmpy($"--port COM{comport} get \"{filePath.Replace("\\", "/")}\"", true);

    public static async Task<bool> ReadFileOnBoardIntoFileAsync(int comport, string filePath, string destPath)
        => (await TryRunAmpy($"--port COM{comport} get \"{filePath}\" \"{destPath.Replace("\\", "/")}\"", false)).success;

    public static async Task<bool> WriteToBoardAsync(int comport, string fileOrDirPath, string destPath)
        => (await TryRunAmpy($"--port COM{comport} put \"{fileOrDirPath}\" \"{destPath.Replace("\\", "/")}\"", false)).success;

    public static async Task<bool> CreateDirectoryAsync(int comport, string newDirPath)
        => (await TryRunAmpy($"--port COM{comport} mkdir \"{newDirPath.Replace("\\", "/")}\"", false)).success;

    public static async Task<(bool success, string[]?)> ListFilesOnBoardAsync(int comport, string dirPath = "/") {
        (bool success, string? output) = await TryRunAmpy($"--port COM{comport} ls \"{dirPath.Replace("\\", "/")}\"", true);
        if (string.IsNullOrEmpty(output) || !success)
            return (false, null);
        return (true, output[1..].Trim().Split("\r\n/"));
    }

    public static class FileRunner {

        private static Process? _runnerProcess;

        public static async void BeginRunningFile(int comport, string filePath) {
            if (IsBusy) {
                ErrorMessages.AmpyIsBusy();
                return;
            }

            IsBusy = true;

            using (_runnerProcess = new() { StartInfo = AmpyDefaultStartInfo }) {
                _runnerProcess.StartInfo.Arguments = $"--port COM{comport} run \"{filePath.Replace("\\", "/")}\"";
                _runnerProcess.EnableRaisingEvents = true;
                _runnerProcess.OutputDataReceived += (s, e) => AmpyOutputDataReceived?.Invoke(s, e);
                _runnerProcess.ErrorDataReceived += (s, e) => AmpyErrorDataReceived?.Invoke(s, e);
                _runnerProcess.Exited += (s, e) => AmpyExited?.Invoke(s, e);
                _runnerProcess.Start();
                _runnerProcess.BeginOutputReadLine();
                _runnerProcess.BeginErrorReadLine();
                await _runnerProcess.WaitForExitAsync();
            }

            _runnerProcess = null;
            IsBusy = false;
        }

        public static void WriteLineToRunningFileInput(string text) {
            if (IsBusy && _runnerProcess != null)
                _runnerProcess.StandardInput.WriteLine(text);
        }

        public static void KillProcess() {
            if (_runnerProcess == null)
                return;

            _runnerProcess.Kill();
            _runnerProcess.Close();
            _runnerProcess = null;
            IsBusy = false;
        }
    }

    public static async Task<bool> RemoveFileFromBoardAsync(int comport, string filePath)
        => (await TryRunAmpy($"--port COM{comport} rm \"{filePath.Replace("\\", "/")}\"", false)).success;

    public static async Task<bool> RemoveDirectoryFromBoardAsync(int comport, string dirPath)
        => (await TryRunAmpy($"--port COM{comport} rmdir \"{dirPath.Replace("\\", "/")}\"", false)).success;

    public static async Task<bool> SoftReset(int comport)
        => (await TryRunAmpy($"--port COM{comport} reset", false)).success;

    public static async Task<bool> DownloadDirectoryFromBoardAsync(int comport, string dirPath, string destDirPath) {

        if (IsBusy) {
            ErrorMessages.AmpyIsBusy();
            return false;
        }

        (bool success, string[]? subPaths) = await ListFilesOnBoardAsync(comport, dirPath);

        if (subPaths == null || !success)
            return false;

        for (int i = 0; i < subPaths.Length && success; ++i) {
            string subPath = subPaths[i];
            bool isDirectory = !Path.HasExtension(subPath);

            string fullDestPath = Path.Combine(destDirPath, subPath);

            if (isDirectory) {
                if (!Directory.Exists(fullDestPath))
                    Directory.CreateDirectory(fullDestPath);
                success = await DownloadDirectoryFromBoardAsync(comport, subPath, destDirPath);
            } else {
                if (!File.Exists(fullDestPath))
                    File.Create(fullDestPath).Close();
                success = await ReadFileOnBoardIntoFileAsync(comport, subPath, fullDestPath);
            }
        }

        return success;
    }
}