using System.IO;
using PiIDE.Wrappers;
using System.Windows;

namespace PiIDE.Editor.Parts.Explorer.BoardExplorer;

public class BoardFileItem : FileItemBase {

    public string FilePathOnBoard { get; }

    public static int Port => GlobalSettings.Default.SelectedCOMPort;

    private new BoardDirectoryItem ParentDirectory => (BoardDirectoryItem) base.ParentDirectory;

    public BoardFileItem(string fullLocalPath, string pathOnBoard, BoardDirectoryItem parentDirectory, ExplorerBase parentExplorer) : base(fullLocalPath, parentDirectory, parentExplorer) => FilePathOnBoard = pathOnBoard;

    protected override async void Delete_Click(object sender, RoutedEventArgs e) {

        if (!CheckForBoardConnection())
            return;

        if (!Tools.EnableBoardInteractions) {
            MessageBox.Show("Unable to connect to Pi", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        SetStatus("Deleting");
        if (await AmpyWrapper.RemoveFileFromBoardAsync(Port, FilePathOnBoard))
            BasicFileActions.DeleteFile(FilePath);
        UnsetStatus();
    }

    protected override void Paste_Click(object sender, RoutedEventArgs e) => ParentDirectory.Paste_Click(this, e);

    protected override async void RenameFile(string oldPath, string newName) {

        SetStatus("Renaming");

        await AmpyWrapper.RemoveFileFromBoardAsync(Port, FilePathOnBoard);

        await AmpyWrapper.WriteToBoardAsync(Port, oldPath, Path.Combine(ParentDirectory.DirectoryPathOnBoard, newName));

        base.RenameFile(oldPath, newName);

        UnsetStatus();
    }

    public static bool CheckForBoardConnection() {
        if (!Tools.EnableBoardInteractions) {
            MessageBox.Show("Unable to connect to Pi", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        return true;
    }

    protected override void Rename_Click(object sender, RoutedEventArgs e) {
        if (CheckForBoardConnection())
            base.Rename_Click(sender, e);
    }
}