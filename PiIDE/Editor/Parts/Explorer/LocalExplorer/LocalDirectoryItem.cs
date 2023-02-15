using PiIDE.Wrapers;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PiIDE.Editor.Parts.Explorer.LocalExplorer {
    public class LocalDirectoryItem : DirectoryItemBase {

        protected FileSystemWatcher? Watcher;

        public override event FileDeletedEventHandler? OnFileDeleted;
        public override event FileRenamedEventHandler? OnFileRenamed;
        public override event FileClickEventHandler? OnFileClick;

        public LocalDirectoryItem(string fullPath, LocalDirectoryItem? parentDirectory) : base(fullPath, parentDirectory) {
            MenuItem newItem = new MenuItem() {
                Header = "Upload to Pi/",
                Icon = new FontAwesome.WPF.FontAwesome() {
                    Icon = FontAwesome.WPF.FontAwesomeIcon.Upload,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
            newItem.Click += Upload_Click;
            DirContextMenu.Items.Add(newItem);
        }

        protected override void Collapse() {
            if (Watcher is not null) {
                Watcher.Dispose();
                Watcher = null;
            }
            ChildrenStackPanel.Children.Clear();
            IsExpandedTextBlock.Text = ">";
        }

        protected override void Expand() {
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

            Watcher.Created += (s, e) => Dispatcher.Invoke(ReloadContent);
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

            for (int i = 0; i < subDirPaths.Length; i++) {
                LocalDirectoryItem item = new(subDirPaths[i], this);
                item.OnFileClick += (s) => OnFileClick?.Invoke(s);
                item.OnFileDeleted += (s, path) => OnFileDeleted?.Invoke(s, path);
                item.OnFileRenamed += (s, oldPath, newPath) => OnFileRenamed?.Invoke(s, oldPath, newPath);
                ChildrenStackPanel.Children.Add(item);
            }

            for (int i = 0; i < subFilePaths.Length; i++) {
                LocalFileItem item = new(subFilePaths[i], this);
                item.OnClick += (s) => OnFileClick?.Invoke(s);
                ChildrenStackPanel.Children.Add(item);
            }

            IsExpandedTextBlock.Text = "V";
        }

        protected void Watcher_Renamed(object sender, RenamedEventArgs e) {
            Dispatcher.Invoke(() => {
                OnFileRenamed?.Invoke(this, e.OldFullPath, e.FullPath);
                ReloadContent();
            });
        }

        protected void Watcher_Deleted(object sender, FileSystemEventArgs e) {
            Dispatcher.Invoke(() => {
                OnFileDeleted?.Invoke(this, e.FullPath);
                ReloadContent();
            });
        }

        protected override void Copy_Click(object sender, RoutedEventArgs e) => FileCopier.Copy(DirectoryPath, true);

        protected override void Cut_Click(object sender, RoutedEventArgs e) => FileCopier.Cut(DirectoryPath, true);

        protected override void Delete_Click(object sender, RoutedEventArgs e) => BasicFileActions.DeleteDirectory(DirectoryPath);

        protected override void Paste_Click(object sender, RoutedEventArgs e) => FileCopier.Paste(DirectoryPath);

        protected override void RenameDirectory(string oldPath, string newPath, string newName) => BasicFileActions.RenameDirectory(oldPath, newName);

        private async void Upload_Click(object sender, RoutedEventArgs e) {
            if (!Tools.EnableBoardInteractions) {
                MessageBox.Show("Unable to connect to Pi", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SetStatus("Uploading");
            await AmpyWraper.WriteToBoardAsync(GlobalSettings.Default.SelectedCOMPort, DirectoryPath, $"/{DirectoryName}");
            BasicFileActions.CopyDirectory(DirectoryPath, Path.Combine(GlobalSettings.Default.LocalBoardFilesPath, DirectoryName));
            UnsetStatus();
        }
    }
}
