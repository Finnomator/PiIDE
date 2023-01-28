using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

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
        private readonly FileSystemWatcher? _fileSystemWatcher;

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

            if (isDir) {
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
                _fileSystemWatcher.Changed += _fileSystemWatcher_Changed;
                _fileSystemWatcher.Created += _fileSystemWatcher_Created;
                _fileSystemWatcher.Deleted += _fileSystemWatcher_Deleted;
                _fileSystemWatcher.Renamed += _fileSystemWatcher_Renamed;
            }

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

        private void _fileSystemWatcher_Renamed(object sender, RenamedEventArgs e) {
            OnFileRenamed?.Invoke(this, e.OldFullPath, e.FullPath);
            Dispatcher.Invoke(ReloadContent);
        }

        private void _fileSystemWatcher_Deleted(object sender, FileSystemEventArgs e) {
            OnFileDeleted?.Invoke(this, e.FullPath);
            Dispatcher.Invoke(ReloadContent);
        }

        private void _fileSystemWatcher_Created(object sender, FileSystemEventArgs e) {
            Dispatcher.Invoke(ReloadContent);
        }

        private void _fileSystemWatcher_Changed(object sender, FileSystemEventArgs e) {
            Dispatcher.Invoke(ReloadContent);
        }

        private void Collapse() {
            Debug.Assert(IsDir);
            MainStackPanel.Children.Clear();
            MainButton.Content = MainButtonCollapsedContent;
        }

        private void Expand() {
            Debug.Assert(IsDir && MainStackPanel.Children.Count == 0);
            string[] subDirPaths = Directory.GetDirectories(FilePath);
            string[] subFilePaths = Directory.GetFiles(FilePath);

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

        private void ReloadParentContent() {
            // TODO: more efficient reload
            Debug.Assert(ContainingParent is not null);
            ContainingParent.ReloadContent();
        }

        private void ReloadContent() {
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

        private void Copy_Click(object sender, System.Windows.RoutedEventArgs e) {
            FileCopier.Copy(FilePath, IsDir);
        }

        private void Cut_Click(object sender, System.Windows.RoutedEventArgs e) {
            FileCopier.Cut(FilePath, IsDir);
        }

        private void Rename_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (IsDir)
                BasicFileActions.RenameDirectory(FilePath, "TestDir");
            else
                BasicFileActions.RenameFile(FilePath, "TestFile.asdf");
            ReloadParentContent();
        }

        private void Delete_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (IsDir)
                BasicFileActions.DeleteDirectory(FilePath);
            else
                BasicFileActions.DeleteFile(FilePath);
            ReloadParentContent();
        }

        private void Paste_Click(object sender, System.Windows.RoutedEventArgs e) {
            // TODO: when copying the folder into itself it copies itself twice
            if (IsDir) {
                FileCopier.Paste(FilePath);
                ReloadContent();
            } else if (ContainingParent is not null) {
                FileCopier.Paste(ContainingParent.FilePath);
                ReloadParentContent();
            } else
                Debug.Assert(false, "ContainingParent was null");
        }
    }
}
