using PiIDE.Editor.Parts.Explorer.LocalExplorer;
using PiIDE.Wrapers;
using System.IO;
using System.Windows;

namespace PiIDE.Editor.Parts.Explorer.BoardExplorer {
    public class BoardDirectoryItem : DirectoryItemBase {

        public string DirectoryPathOnBoard { get; private set; }

        public override event FileDeletedEventHandler? OnFileDeleted;
        public override event FileRenamedEventHandler? OnFileRenamed;
        public override event FileClickEventHandler? OnFileClick;

        public static int Port => GlobalSettings.Default.SelectedCOMPort;

        public BoardDirectoryItem(string fullPath, string directoryPathOnBoard, BoardDirectoryItem? parentDirectory) : base(fullPath, parentDirectory) {
            DirectoryPathOnBoard = directoryPathOnBoard;
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
                BoardDirectoryItem item = new(subDirPaths[i], Path.Combine(DirectoryPathOnBoard, Path.GetFileName(subDirPaths[i])), this);
                item.OnFileClick += (s) => OnFileClick?.Invoke(s);
                item.OnFileDeleted += (s, path) => OnFileDeleted?.Invoke(s, path);
                item.OnFileRenamed += (s, oldPath, newPath) => OnFileRenamed?.Invoke(s, oldPath, newPath);
                ChildrenStackPanel.Children.Add(item);
            }

            for (int i = 0; i < subFilePaths.Length; i++) {
                BoardFileItem item = new(subFilePaths[i], Path.Combine(DirectoryPathOnBoard, Path.GetFileName(subFilePaths[i])), this);
                item.OnClick += (s) => OnFileClick?.Invoke(s);
                ChildrenStackPanel.Children.Add(item);
            }
        }

        // TODO: Implement these features
        // But make sure that all paths get changed correctly
        protected override void Copy_Click(object sender, RoutedEventArgs e) => ErrorMessager.FeatureNotSupported();

        protected override void Cut_Click(object sender, RoutedEventArgs e) => ErrorMessager.FeatureNotSupported();

        protected override async void Delete_Click(object sender, RoutedEventArgs e) {

            if (!CheckForBoardConnection())
                return;

            SetStatus("Deleting");
            await AmpyWraper.RemoveDirectoryFromBoardAsync(Port, DirectoryPathOnBoard);
            BasicFileActions.DeleteDirectory(DirectoryPath);
            UnsetStatus();
        }

        protected override void Paste_Click(object sender, RoutedEventArgs e) => ErrorMessager.FeatureNotSupported();

        protected override void RenameDirectory(string oldPath, string newPath, string newName) => ErrorMessager.FeatureNotSupported();

        // TODO: this method should not be overriden, but as long as the feature is not supported, it will
        protected override void Rename_Click(object sender, RoutedEventArgs e) => ErrorMessager.FeatureNotSupported();

        public static bool CheckForBoardConnection() {
            if (!Tools.EnableBoardInteractions) {
                MessageBox.Show("Unable to connect to Pi", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
    }
}
