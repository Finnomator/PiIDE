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

        private int Indent;
        private string FileName;
        private const string ExpandedChar = "V";
        private const string CollapsedChar = ">";

        private string MainButtonCollapsedContent;
        private string MainButtonExpandedContent;

        public FileViewItem() {
            InitializeComponent();
        }

        public FileViewItem(bool isDir, string filePath, int indent) {
            InitializeComponent();
            OpenDir(isDir, filePath, indent);
        }

        public void OpenDir(bool isDir, string filePath, int indent) {
            IsDir = isDir;
            FilePath = filePath;
            Indent = indent;
            FileName = Path.GetFileName(filePath).Replace("_", "__");

            string space = new string(' ', Indent * 2);

            if (isDir) {
                MainButtonCollapsedContent = $"{space}{CollapsedChar} {FileName}";
                MainButtonExpandedContent = $"{space}{ExpandedChar} {FileName}";
                MainButton.Content = MainButtonCollapsedContent;
            } else {
                MainButton.Content = $"{space}{FileName}";
            }
        }

        public void Collapse() {
            Debug.Assert(IsDir);
            MainStackPanel.Children.Clear();
            MainButton.Content = MainButtonCollapsedContent;
        }

        public void Expand() {
            Debug.Assert(IsDir);
            string[] subDirPaths = Directory.GetDirectories(FilePath);
            string[] subFilePaths = Directory.GetFiles(FilePath);

            for (int i = 0; i < subDirPaths.Length; i++) {
                FileViewItem fileViewItem = new(true, subDirPaths[i], Indent + 1);
                fileViewItem.OnFileClick += (s, e) => OnFileClick?.Invoke(s, e);
                MainStackPanel.Children.Add(fileViewItem);
            }

            for (int i = 0; i < subFilePaths.Length; i++) {
                FileViewItem fileViewItem = new(false, subFilePaths[i], Indent + 1);
                fileViewItem.OnFileClick += (s, e) => OnFileClick?.Invoke(s, e);
                MainStackPanel.Children.Add(fileViewItem);
            }

            MainButton.Content = MainButtonExpandedContent;
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
    }
}
