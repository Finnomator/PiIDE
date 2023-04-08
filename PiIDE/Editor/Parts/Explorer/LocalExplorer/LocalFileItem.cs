using PiIDE.Wrapers;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PiIDE.Editor.Parts.Explorer.LocalExplorer;

public class LocalFileItem : FileItemBase {

    public LocalFileItem(string fullPath, DirectoryItemBase parentDirectory, ExplorerBase parentExplorer) : base(fullPath, parentDirectory, parentExplorer) {
        MenuItem newItem = new() {
            Header = "Upload to Pi/",
            Icon = new FontAwesome.WPF.FontAwesome() {
                Icon = FontAwesome.WPF.FontAwesomeIcon.Upload,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        newItem.Click += Upload_Click;
        FileContextMenu.Items.Add(newItem);
    }

    private async void Upload_Click(object sender, RoutedEventArgs e) {
        if (!Tools.EnableBoardInteractions) {
            MessageBox.Show("Unable to connect to Pi", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        SetStatus("Uploading");
        if (await AmpyWraper.WriteToBoardAsync(GlobalSettings.Default.SelectedCOMPort, FilePath, $"/{FileName}"))
            BasicFileActions.CopyFile(FilePath, Path.Combine(GlobalSettings.Default.LocalBoardFilesPath, FileName));
        UnsetStatus();
    }
}