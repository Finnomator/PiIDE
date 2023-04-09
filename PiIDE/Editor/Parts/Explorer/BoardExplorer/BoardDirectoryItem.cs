using System.Diagnostics;
using PiIDE.Wrappers;
using System.IO;
using System.Windows;

namespace PiIDE.Editor.Parts.Explorer.BoardExplorer;

public class BoardDirectoryItem : DirectoryItemBase {

    public string DirectoryPathOnBoard { get; }
    private new BoardDirectoryItem? ParentDirectory => (BoardDirectoryItem?) base.ParentDirectory;

    public static int Port => GlobalSettings.Default.SelectedCOMPort;

    public BoardDirectoryItem(string fullPath, string directoryPathOnBoard, ExplorerBase parentExplorer) : base(fullPath, parentExplorer) {
        DirectoryPathOnBoard = directoryPathOnBoard;

        DirContextMenu.Items.Remove(RenameMenuItem);
        DirContextMenu.Items.Remove(CopyMenuItem);
        DirContextMenu.Items.Remove(DeleteMenuItem);
        DirContextMenu.Items.Remove(CutMenuItem);
    }

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

    protected override async void Delete_Click(object sender, RoutedEventArgs e) {

        if (!CheckForBoardConnection())
            return;

        SetStatus("Deleting");
        if (await AmpyWrapper.RemoveDirectoryFromBoardAsync(Port, DirectoryPathOnBoard))
            BasicFileActions.DeleteDirectory(DirectoryPath);
        UnsetStatus();
    }

    protected override async void Paste_Click(object sender, RoutedEventArgs e) {
        (_, string? newPastedFilePath) = FileCopier.Paste(DirectoryPath);
        if (newPastedFilePath == null)
            return;
        SetStatus("Pasting");
        await AmpyWrapper.WriteToBoardAsync(Port, newPastedFilePath, Path.Combine(DirectoryPathOnBoard, Path.GetFileName(newPastedFilePath!)));
        UnsetStatus();
    }

    protected override async void RenameDirectory(string oldPath, string newName) {

        if (ParentDirectory == null)
            return;

        SetStatus("Renaming");

        await AmpyWrapper.RemoveDirectoryFromBoardAsync(Port, DirectoryPathOnBoard);

        await AmpyWrapper.WriteToBoardAsync(Port, oldPath, Path.Combine(ParentDirectory.DirectoryPathOnBoard, newName));

        base.RenameDirectory(oldPath, newName);

        UnsetStatus();
    }

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

    public static bool CheckForBoardConnection() {
        if (!Tools.EnableBoardInteractions) {
            MessageBox.Show("Unable to connect to Pi", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        return true;
    }
}