using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace PiIDE {

    public partial class FileViewItem : UserControl {

        public bool IsDir { get; private set; }
        public string FilePath { get; private set; }
        public bool IsExpanded { get; private set; }
        public EventHandler<string>? OnFileClick;

        private readonly int Indent;
        private readonly string FileName;
        private const string ExpandedChar = "V";
        private const string CollapsedChar = ">";
        private readonly FileViewItem? ContainingParent;

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
            FileName = Path.GetFileName(filePath).Replace("_", "__");

            string space = new(' ', Indent * 2);

            if (isDir) {
                MainButtonCollapsedContent = $"{space}{CollapsedChar} {FileName}";
                MainButtonExpandedContent = $"{space}{ExpandedChar} {FileName}";
                MainButton.Content = MainButtonCollapsedContent;
            } else {
                MainButton.Content = $"{space}{FileName}";
            }
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
                fileViewItem.OnFileClick += (s, e) => OnFileClick?.Invoke(s, e);
                MainStackPanel.Children.Add(fileViewItem);
            }

            for (int i = 0; i < subFilePaths.Length; i++) {
                FileViewItem fileViewItem = new(false, subFilePaths[i], Indent + 1, this);
                fileViewItem.OnFileClick += (s, e) => OnFileClick?.Invoke(s, e);
                MainStackPanel.Children.Add(fileViewItem);
            }

            MainButton.Content = MainButtonExpandedContent;
        }

        private void ReloadParentContent() {
            // TODO: more efficient reload
            Debug.Assert(ContainingParent is not null);
            MainStackPanel.Children.Clear();
            ContainingParent.Expand();
        }

        private void MainButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            if (IsDir) {
                if (IsExpanded)
                    Collapse();
                else
                    Expand();
                IsExpanded = !IsExpanded;
                return;
            }
            OnFileClick?.Invoke(sender, FilePath);
        }

        private void Copy_Click(object sender, System.Windows.RoutedEventArgs e) {
            FileActions.Copy(FilePath);
        }

        private void Cut_Click(object sender, System.Windows.RoutedEventArgs e) {
            FileActions.Copy(FilePath);
        }

        private void Rename_Click(object sender, System.Windows.RoutedEventArgs e) {
            
        }

        private void Delete_Click(object sender, System.Windows.RoutedEventArgs e) {
            FileActions.Delete(FilePath, IsDir);
            ReloadParentContent();
        }
    }
}
