using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace PiIDE {
    internal static class BasicFileActions {
        public static void CopyFile(string sourcePath, string destPath) {
            string newDestPath = destPath;
            string newDestPathWithoutExt = newDestPath[..^Path.GetExtension(newDestPath).Length];
            string newDestPathExt = Path.GetExtension(newDestPath);

            for (int i = 1; File.Exists(newDestPath); i++)
                newDestPath = $"{newDestPathWithoutExt}{i}{newDestPathExt}";

            try {
                File.Copy(sourcePath, newDestPath);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to Copy File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void CopyDirectory(string sourceDir, string destinationDir) {

            string temp = destinationDir;
            for (int i = 0; Directory.Exists(destinationDir); i++)
                destinationDir = $"{temp}{i}";

            try {
                PCopyDirectory(sourceDir, destinationDir);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to Copy Directory", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void PCopyDirectory(string sourcePath, string targetPath) {
            Directory.CreateDirectory(targetPath);
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        public static void DeleteDirectory(string dirPath) {
            try {
                Directory.Delete(dirPath, true);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to Delete Directory", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void DeleteFile(string filePath) {
            try {
                File.Delete(filePath);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to Delete File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void MoveDirectory(string oldDirPath, string newDirPath) {
            try {
                Directory.Move(oldDirPath, newDirPath);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to Move Directory", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void MoveFile(string oldFilePath, string newFilePath) {
            try {
                Directory.Move(oldFilePath, newFilePath);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to Move File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void RenameFile(string oldFilePath, string newFileName) {
            FileInfo fileInfo = new(oldFilePath);
            string newFilePath;
            if (fileInfo.Directory is null)
                newFilePath = newFileName;
            else
                newFilePath = Path.Combine(fileInfo.Directory.FullName, newFileName);
            MoveFile(oldFilePath, newFilePath);
        }

        public static void RenameDirectory(string oldDirPath, string newDirName) {
            DirectoryInfo dirInfo = new(oldDirPath);
            string newDirPath;
            if (dirInfo.Parent is null)
                newDirPath = newDirName;
            else
                newDirPath = Path.Combine(dirInfo.Parent.FullName, newDirName);
            MoveFile(oldDirPath, newDirPath);
        }
    }

    internal static class FileCopier {
        private static string? _copiedPath;
        private static bool _cut;
        private static bool _isDir;

        public static void Copy(string path, bool isDir) {
            _isDir = isDir;
            _copiedPath = path;
        }

        public static void Cut(string path, bool isDir) {
            _cut = true;
            _isDir = isDir;
            Copy(path, isDir);
        }

        public static void Paste(string destParentDirectory) {

            Debug.Assert(_copiedPath is not null);

            string baseName = Path.GetFileName(_copiedPath);
            string destPath = Path.Combine(destParentDirectory, baseName);

            if (_isDir) {
                if (_cut)
                    BasicFileActions.MoveDirectory(_copiedPath, destPath);
                else
                    BasicFileActions.CopyDirectory(_copiedPath, destPath);
            } else {
                if (_cut)
                    BasicFileActions.MoveFile(_copiedPath, destPath);
                else
                    BasicFileActions.CopyFile(_copiedPath, destPath);
            }
        }
    }
}
