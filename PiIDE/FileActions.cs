﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace PiIDE;

internal static class BasicFileActions {

    public static string? CopyFile(string sourcePath, string destPath) {
        string newDestPath = destPath;
        string newDestPathWithoutExt = newDestPath[..^Path.GetExtension(newDestPath).Length];
        string newDestPathExt = Path.GetExtension(newDestPath);

        for (int i = 1; File.Exists(newDestPath); i++)
            newDestPath = $"{newDestPathWithoutExt}{i}{newDestPathExt}";

        try {
            File.Copy(sourcePath, newDestPath);

        } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Failed to Copy File", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        return null;
    }

    public static string? CopyDirectory(string sourceDir, string destinationDir, bool renameIfExists, bool replaceIfExists) {

        if (renameIfExists) {
            string temp = destinationDir;
            for (int i = 0; Directory.Exists(destinationDir); i++)
                destinationDir = $"{temp}{i}";
        } else if (Directory.Exists(destinationDir) && replaceIfExists)
            Directory.Delete(destinationDir, true);

        try {
            PCopyDirectory(sourceDir, destinationDir);
        } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Failed to Copy Directory", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        return destinationDir;
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

    public static bool DeleteDirectory(string dirPath) {
        try {
            Directory.Delete(dirPath, true);
        } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Failed to Delete Directory", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        return true;
    }

    public static bool DeleteFile(string filePath) {
        try {
            File.Delete(filePath);
        } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Failed to Delete File", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        return true;
    }

    public static bool MoveDirectory(string oldDirPath, string newDirPath) {
        try {
            Directory.Move(oldDirPath, newDirPath);
        } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Failed to Move Directory", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        return true;
    }

    public static bool MoveFile(string oldFilePath, string newFilePath) {
        try {
            Directory.Move(oldFilePath, newFilePath);
        } catch (Exception ex) {
            MessageBox.Show(ex.Message, "Failed to Move File", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        return true;
    }

    public static bool RenameFile(string oldFilePath, string newFileName) {
        FileInfo fileInfo = new(oldFilePath);
        string newFilePath = fileInfo.Directory == null ? newFileName : Path.Combine(fileInfo.Directory.FullName, newFileName);
        return MoveFile(oldFilePath, newFilePath);
    }

    public static bool RenameDirectory(string oldDirPath, string newDirName) {
        DirectoryInfo dirInfo = new(oldDirPath);
        string newDirPath = dirInfo.Parent == null ? newDirName : Path.Combine(dirInfo.Parent.FullName, newDirName);
        return MoveFile(oldDirPath, newDirPath);
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

    public static (string? sourceFilePath, string? newPastedFilePath, bool? cut, bool? wasDir) Paste(string destParentDirectory) {

        if (_copiedPath == null)
            return (null, null, null, null);

        string? destPath = Path.Combine(destParentDirectory, Path.GetFileName(_copiedPath));

        switch (_cut) {
            case true when destPath == _copiedPath:
                return (null, null, null, null);
            case true when Path.GetPathRoot(_copiedPath) != Path.GetPathRoot(destPath):
                MessageBox.Show("Cannot move files across different volumes. Use copy instead", "Cannot move file",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return (null, null, null, null);
        }

        bool success = false;

        if (_isDir) {
            if (_cut)
                success = BasicFileActions.MoveDirectory(_copiedPath, destPath);
            else
                destPath = BasicFileActions.CopyDirectory(_copiedPath, destPath, true, false);
        } else {
            if (_cut)
                success = BasicFileActions.MoveFile(_copiedPath, destPath);
            else
                destPath = BasicFileActions.CopyFile(_copiedPath, destPath);
        }

        return success ? (_copiedPath, destPath, _cut, _isDir) : (null, null, null, null);
    }
}