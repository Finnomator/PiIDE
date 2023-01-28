using System;
using System.IO;
using System.Windows;

namespace PiIDE {
    internal static class FileActions {

        private static string? _copiedPath;

        public static void Copy(string Path) => _copiedPath = Path;

        public static void Paste(string filePath, bool isDirectory, bool cut = false) {

            if (string.IsNullOrEmpty(_copiedPath))
                return;

            if (filePath == _copiedPath && cut)
                return;

            string newFilePath = filePath;
            string newFilePathWithoutExt = newFilePath;
            string newFilePathExt = "";

            if (!isDirectory) {
                newFilePathWithoutExt = Path.GetFileNameWithoutExtension(newFilePath);
                newFilePathExt = Path.GetExtension(newFilePathWithoutExt);
            }

            for (int i = 1; File.Exists(newFilePath); i++)
                newFilePath = newFilePathWithoutExt + i + newFilePathExt;

            if (isDirectory) {
                if (cut)
                    MoveDirectory(newFilePath);
                else
                    CopyDirectory(_copiedPath, newFilePath);
            } else {
                if (cut)
                    MoveFile(newFilePath);
                else
                    PasteFile(newFilePath);
            }
        }

        public static void Delete(string Path, bool isDirectory) {
            if (isDirectory)
                DeleteDirectory(Path);
            else
                DeleteFile(Path);
        }

        public static void Cut(string Path) => Paste(Path, true);

        private static void DeleteDirectory(string dirPath) {
            try {
                Directory.Delete(dirPath, true);
            } catch (Exception ex) {
#if DEBUG
                throw;
#else
                MessageBox.Show($"Failed to delete directory '{dirPath}'\r\n{ex.Message}", "Failed to Delete Directory", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void PasteFile(string filePath) {
            try {
                File.Copy(_copiedPath, filePath);
            } catch (Exception ex) {
#if DEBUG
                throw;
#else
                MessageBox.Show($"Failed to copy file '{filePath}'\r\n{ex.Message}", "Failed to Copy File", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true) {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles()) {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive) {
                foreach (DirectoryInfo subDir in dirs) {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        private static void DeleteFile(string filePath) {
            try {
                File.Delete(filePath);
            } catch (Exception ex) {
#if DEBUG
                throw;
#else
                MessageBox.Show($"Failed to delete file '{filePath}'\r\n{ex.Message}", "Failed to Delete File", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void MoveDirectory(string newDirPath) {
            try {
                Directory.Move(_copiedPath, newDirPath);
            } catch (Exception ex) {
#if DEBUG
                throw;
#else
                MessageBox.Show($"Failed to move directory '{newDirPath}'\r\n{ex.Message}", "Failed to Move Directory", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        private static void MoveFile(string newFilePath) {
            try {
                Directory.Move(_copiedPath, newFilePath);
            } catch (Exception ex) {
#if DEBUG
                throw;
#else
                MessageBox.Show($"Failed to move file '{newFilePath}'\r\n{ex.Message}", "Failed to Move File", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }
    }
}
