using PiIDE.Wrapers;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PiIDE.Editor.Parts.Explorer.LocalExplorer {
    public class LocalFileItem : FileItemBase {

        public LocalFileItem(string fullPath, DirectoryItemBase parentDirectory) : base(fullPath, parentDirectory) {
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

        protected override void Copy_Click(object sender, RoutedEventArgs e) => FileCopier.Copy(FilePath, false);

        protected override void Cut_Click(object sender, RoutedEventArgs e) => FileCopier.Cut(FilePath, false);

        protected override void Delete_Click(object sender, RoutedEventArgs e) => BasicFileActions.DeleteFile(FilePath);

        protected override void Paste_Click(object sender, RoutedEventArgs e) => FileCopier.Paste(ParentDirectory.DirectoryPath);

        protected override void RenameFile(string oldPath, string newPath, string newName) => BasicFileActions.RenameFile(oldPath, newName);

        private async void Upload_Click(object sender, RoutedEventArgs e) {
            if (!Tools.EnableBoardInteractions) {
                MessageBox.Show("Unable to connect to Pi", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SetStatus("Uploading");
            await AmpyWraper.WriteToBoardAsync(GlobalSettings.Default.SelectedCOMPort, FilePath, $"/{FileName}");
            BasicFileActions.CopyFile(FilePath, Path.Combine(GlobalSettings.Default.LocalBoardFilesPath, FileName));
            UnsetStatus();
        }
    }
}
