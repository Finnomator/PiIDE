using PiIDE.Wrapers;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PiIDE.Editor.Parts.Explorer.LocalExplorer {
    public class LocalDirectoryItem : DirectoryItemBase {

        public LocalDirectoryItem(string fullPath, ExplorerBase parentExplorer) : base(fullPath, parentExplorer) {
            Init();
        }

        private LocalDirectoryItem(string fullPath, DirectoryItemBase parentDirectory, ExplorerBase parentExplorer) : base(fullPath, parentDirectory, parentExplorer) {
            Init();
        }

        private void Init() {
            MenuItem newItem = new() {
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
                LocalDirectoryItem item = new(subDirPaths[i], this, ParentExplorer);
                ChildrenStackPanel.Children.Add(item);
            }

            for (int i = 0; i < subFilePaths.Length; i++) {
                LocalFileItem item = new(subFilePaths[i], this, ParentExplorer);
                ChildrenStackPanel.Children.Add(item);
            }
        }

        private async void Upload_Click(object sender, RoutedEventArgs e) {
            if (!Tools.EnableBoardInteractions) {
                MessageBox.Show("Unable to connect to Pi", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SetStatus("Uploading");
            if (await AmpyWraper.WriteToBoardAsync(GlobalSettings.Default.SelectedCOMPort, DirectoryPath, $"/{DirectoryName}"))
                BasicFileActions.CopyDirectory(DirectoryPath, Path.Combine(GlobalSettings.Default.LocalBoardFilesPath, DirectoryName));
            UnsetStatus();
        }
    }
}
