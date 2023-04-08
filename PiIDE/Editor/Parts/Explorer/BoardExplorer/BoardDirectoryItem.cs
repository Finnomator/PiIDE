﻿using PiIDE.Wrappers;
using System.IO;
using System.Windows;

namespace PiIDE.Editor.Parts.Explorer.BoardExplorer;

public class BoardDirectoryItem : DirectoryItemBase {

    public string DirectoryPathOnBoard { get; }

    public static int Port => GlobalSettings.Default.SelectedCOMPort;

    public BoardDirectoryItem(string fullPath, string directoryPathOnBoard, ExplorerBase parentExplorer) : base(fullPath, parentExplorer) => DirectoryPathOnBoard = directoryPathOnBoard;

    private BoardDirectoryItem(string fullPath, string directoryPathOnBoard, ExplorerBase parentExplorer, BoardDirectoryItem parentDirectory) : base(fullPath, parentDirectory, parentExplorer) => DirectoryPathOnBoard = directoryPathOnBoard;

    protected override void Expand() {
        base.Expand();

        Watcher = new(DirectoryPath) {
            NotifyFilter = NotifyFilters.Attributes
                           | NotifyFilters.CreationTime
                           | NotifyFilters.DirectoryName
                           | NotifyFilters.FileName
                           | NotifyFilters.LastAccess
                           | NotifyFilters.LastWrite
                           | NotifyFilters.Security
                           | NotifyFilters.Size,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true,
        };

        Watcher.Created += (_, _) => Dispatcher.Invoke(ReloadContent);
        Watcher.Deleted += Watcher_Deleted;
        Watcher.Renamed += Watcher_Renamed;

        string[] subDirPaths;
        string[] subFilePaths;

        try {
            // TODO: get rid of the catch statement (currently it throws when the parent dir of an open file gets deleted)
            subDirPaths = Directory.GetDirectories(DirectoryPath);
            subFilePaths = Directory.GetFiles(DirectoryPath);
        } catch {
            return;
        }

        foreach (string subDirPath in subDirPaths) {
            BoardDirectoryItem item = new(subDirPath, Path.Combine(DirectoryPathOnBoard, Path.GetFileName(subDirPath)), ParentExplorer, this);
            ChildrenStackPanel.Children.Add(item);
        }

        foreach (string subFilePath in subFilePaths) {
            BoardFileItem item = new(subFilePath, Path.Combine(DirectoryPathOnBoard, Path.GetFileName(subFilePath)), this, ParentExplorer);
            ChildrenStackPanel.Children.Add(item);
        }
    }

    // TODO: Implement these features
    // But make sure that all paths get changed correctly
    protected override void Copy_Click(object sender, RoutedEventArgs e) => ErrorMessages.FeatureNotSupported();

    protected override void Cut_Click(object sender, RoutedEventArgs e) => ErrorMessages.FeatureNotSupported();

    protected override async void Delete_Click(object sender, RoutedEventArgs e) {

        if (!CheckForBoardConnection())
            return;

        SetStatus("Deleting");
        if (await AmpyWrapper.RemoveDirectoryFromBoardAsync(Port, DirectoryPathOnBoard))
            BasicFileActions.DeleteDirectory(DirectoryPath);
        UnsetStatus();
    }

    protected override void Paste_Click(object sender, RoutedEventArgs e) => ErrorMessages.FeatureNotSupported();

    protected override void RenameDirectory(string oldPath, string newName) => ErrorMessages.FeatureNotSupported();

    protected override async void AddFile_Click(object sender, RoutedEventArgs e) {
        string newFileLocalPath = Path.Combine(DirectoryPath, "new_file.py");
        if (Tools.TryCreateFile(newFileLocalPath))
            await AmpyWrapper.WriteToBoardAsync(Port, newFileLocalPath, Path.Combine(DirectoryPathOnBoard, "new_file.py"));
    }

    protected override async void AddFolder_Click(object sender, RoutedEventArgs e) {
        string newDirLocalPath = Path.Combine(DirectoryPath, "NewFolder");
        if (await AmpyWrapper.CreateDirectoryAsync(Port, Path.Combine(DirectoryPathOnBoard, "NewFolder")))
            Tools.TryCreateDirectory(newDirLocalPath);
    }

    // TODO: this method should not be overriden, but as long as the feature is not supported, it will
    protected override void Rename_Click(object sender, RoutedEventArgs e) => ErrorMessages.FeatureNotSupported();

    public static bool CheckForBoardConnection() {
        if (!Tools.EnableBoardInteractions) {
            MessageBox.Show("Unable to connect to Pi", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        return true;
    }
}