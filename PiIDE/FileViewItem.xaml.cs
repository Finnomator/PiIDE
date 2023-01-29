using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PiIDE {

    public partial class FileViewItem : UserControl {

        public bool IsDir { get; private set; }
        public string FilePath { get; private set; }
        public bool IsExpanded { get; private set; }

        public delegate void FileViewItemClickEventHandler(FileViewItem sender);
        public delegate void FileViewItemFileDeletedEventHandler(FileViewItem sender, string deltedFile);
        public delegate void FileViewItemFileRenamedEventHandler(FileViewItem sender, string oldFile, string newFile);
        public FileViewItemClickEventHandler? OnFileClick;
        public FileViewItemFileDeletedEventHandler? OnFileDeleted;
        public FileViewItemFileRenamedEventHandler? OnFileRenamed;

        private readonly int Indent;
        private const string ExpandedChar = "V";
        private const string CollapsedChar = ">";
        private readonly FileViewItem? ContainingParent;
        private FileSystemWatcher? _fileSystemWatcher;

        private readonly string? MainButtonCollapsedContent;
        private readonly string? MainButtonExpandedContent;

        public FileViewItem() : this(true, "C:/", 0, null) {
        }

        public FileViewItem(string directory) : this(true, directory, 0, null) {
        }

        private FileViewItem(bool isDir, string filePath, int indent, FileViewItem? parent) {
            InitializeComponent();
            IsDir = isDir;
            FilePath = filePath;
            Indent = indent;
            ContainingParent = parent;

            

            string buttonContent = Path.GetFileName(filePath).Replace("_", "__");

            string space = new(' ', Indent * 2);

            if (isDir) {
                MainButtonCollapsedContent = $"{space}{CollapsedChar} {buttonContent}";
                MainButtonExpandedContent = $"{space}{ExpandedChar} {buttonContent}";
                MainButton.Content = MainButtonCollapsedContent;
            } else {
                MainButton.Content = $"{space}{buttonContent}";
            }
        }

        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e) {
            Dispatcher.Invoke(() => {
                ReloadContent();
                OnFileRenamed?.Invoke(this, e.OldFullPath, e.FullPath);
            });
        }

        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e) {
            Debug.WriteLine("Deleted " + e.FullPath + " from " + FilePath);
            Dispatcher.Invoke(() => {
                OnFileDeleted?.Invoke(this, e.FullPath);
                ReloadContent();
            });
        }

        private void Collapse() {
            Debug.Assert(IsDir);
            MainStackPanel.Children.Clear();
            MainButton.Content = MainButtonCollapsedContent;
        }

        private void Expand() {
            Debug.Assert(IsDir && MainStackPanel.Children.Count == 0);

            if (IsDir && _fileSystemWatcher is null) {
                // TODO: FileSystemWatcher doesnt like paths with spaces
                _fileSystemWatcher = new FileSystemWatcher(FilePath) {
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
                _fileSystemWatcher.Created += (s, e) => Dispatcher.Invoke(ReloadContent);
                _fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
                _fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
            }

            string[] subDirPaths;
            string[] subFilePaths;

            try {
                // TODO: get rid of the catch statement (currently it throws when the parent dir of an open file gets deleted)
                subDirPaths = Directory.GetDirectories(FilePath);
                subFilePaths = Directory.GetFiles(FilePath);
            } catch {
                return;
            }

            for (int i = 0; i < subDirPaths.Length; i++) {
                FileViewItem fileViewItem = new(true, subDirPaths[i], Indent + 1, this);
                fileViewItem.OnFileClick += (s) => OnFileClick?.Invoke(s);
                fileViewItem.OnFileDeleted += (s, filePath) => OnFileDeleted?.Invoke(s, filePath);
                fileViewItem.OnFileRenamed += (s, oldPath, newPath) => OnFileRenamed?.Invoke(s, oldPath, newPath);
                MainStackPanel.Children.Add(fileViewItem);
            }

            for (int i = 0; i < subFilePaths.Length; i++) {
                FileViewItem fileViewItem = new(false, subFilePaths[i], Indent + 1, this);
                fileViewItem.OnFileClick += (s) => OnFileClick?.Invoke(s);
                fileViewItem.OnFileDeleted += (s, filePath) => OnFileDeleted?.Invoke(s, filePath);
                fileViewItem.OnFileRenamed += (s, oldPath, newPath) => OnFileRenamed?.Invoke(s, oldPath, newPath);
                MainStackPanel.Children.Add(fileViewItem);
            }

            MainButton.Content = MainButtonExpandedContent;
        }

        private void ReloadContent() {
            // TODO: more efficient reload
            MainStackPanel.Children.Clear();
            Expand();
        }

        private void MainButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (IsDir) {
                if (IsExpanded)
                    Collapse();
                else
                    Expand();
                IsExpanded = !IsExpanded;
            }
            OnFileClick?.Invoke(this);
        }

        private void Copy_Click(object sender, System.Windows.RoutedEventArgs e) => FileCopier.Copy(FilePath, IsDir);

        private void Cut_Click(object sender, System.Windows.RoutedEventArgs e) => FileCopier.Cut(FilePath, IsDir);

        private void Rename_Click(object sender, System.Windows.RoutedEventArgs e) {
            RenameTextBox.Text = (string) MainButton.Content;
            RenameTextBox.Visibility = Visibility.Visible;
            RenameTextBox.Focus();
            RenameTextBox.SelectAll();
        }

        private void Delete_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (IsDir)
                BasicFileActions.DeleteDirectory(FilePath);
            else
                BasicFileActions.DeleteFile(FilePath);
        }

        private void Paste_Click(object sender, System.Windows.RoutedEventArgs e) {
            // TODO: when copying the folder into itself it copies itself twice
            if (IsDir) {
                FileCopier.Paste(FilePath);
            } else if (ContainingParent is not null) {
                FileCopier.Paste(ContainingParent.FilePath);
            } else
                Debug.Assert(false, "ContainingParent was null");
        }

        private void RenameTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e) => RenameTextBox.Visibility = Visibility.Collapsed;

        private void RenameTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            switch (e.Key) {
                case System.Windows.Input.Key.Enter:
                    RenameTextBoxConfirmation();
                    break;
                case System.Windows.Input.Key.Escape:
                    RenameTextBox.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void RenameTextBoxConfirmation() {

            string oldName = (string) MainButton.Content;
            string newName = RenameTextBox.Text;
            string newPath = Path.Combine(FilePath[^newName.Length..], newName);

            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) {
                MessageBox.Show("Invalid characters in path", "Renaming Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RenameTextBox.Visibility = Visibility.Collapsed;
                return;
            }

            if (IsDir) {
                if (Directory.Exists(newPath)) {
                    MessageBox.Show("The directory already exists", "Renaming Error", MessageBoxButton.OK, MessageBoxImage.Error);
                } else if (oldName == newName) {
                } else {
                    BasicFileActions.RenameDirectory(FilePath, newName);
                }
            } else {
                if (File.Exists(newPath)) {
                    MessageBox.Show("The file already exists", "Renaming Error", MessageBoxButton.OK, MessageBoxImage.Error);
                } else if (oldName == newName) {
                } else {
                    BasicFileActions.RenameFile(FilePath, newName);
                }
            }

            RenameTextBox.Visibility = Visibility.Collapsed;
        }
    }
}
