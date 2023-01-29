using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PiIDE {

    public partial class BoardFileViewItem : UserControl {

        public bool IsDir { get; private set; }
        public string BoardFilePath { get; private set; }
        public bool IsExpanded { get; private set; }
        public EventHandler<string>? OnFileClick;
        public int COMPort { get; private set; }
        public bool IsRootDir { get; private set; }
        public bool DirectoryHasBeenDownloadedOnce { get; private set; }
        public string LocalFilePath { get; private set; }
        public string LocalRootDirPath { get; private set; }

        private int Indent;
        private string FileName;
        private const string ExpandedChar = "V";
        private const string CollapsedChar = ">";
        private const string RootDirTitle = "Pi";
        private string[] LocalSubFilePaths;
        private string[] LocalSubDirPaths;

        private string MainButtonCollapsedContent;
        private string MainButtonExpandedContent;

        public BoardFileViewItem(string boardFilePath, string localRootDirPath, int comPort, int indent) {
            InitializeComponent();
            IsDir = !Path.HasExtension(boardFilePath);
            BoardFilePath = boardFilePath;
            LocalRootDirPath = localRootDirPath;
            LocalFilePath = Path.Combine(LocalRootDirPath, boardFilePath);
            COMPort = comPort;
            Indent = indent;

            FileName = Path.GetFileName(LocalFilePath).Replace("_", "__");
            IsRootDir = FileName == "";

            string space = new(' ', Indent * 2);

            if (IsDir) {
                LocalSubDirPaths = Directory.GetDirectories(LocalFilePath);
                LocalSubFilePaths = Directory.GetFiles(LocalFilePath);
                MainButtonCollapsedContent = $"{space}{CollapsedChar} {(IsRootDir ? RootDirTitle : FileName)}";
                MainButtonExpandedContent = $"{space}{ExpandedChar} {(IsRootDir ? RootDirTitle : FileName)}";
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

            for (int i = 0; i < LocalSubDirPaths.Length; i++) {
                string localSubPath = LocalSubDirPaths[i];
                string boardSubPath = localSubPath[LocalRootDirPath.Length..];
                BoardFileViewItem fileViewItem = new(boardSubPath, LocalRootDirPath, COMPort, Indent + 1);
                fileViewItem.OnFileClick += (s, e) => OnFileClick?.Invoke(s, e);
                MainStackPanel.Children.Add(fileViewItem);
            }

            for (int i = 0; i < LocalSubFilePaths.Length; i++) {
                string localSubPath = LocalSubFilePaths[i];
                string boardSubPath = localSubPath[LocalRootDirPath.Length..];
                BoardFileViewItem fileViewItem = new(boardSubPath, LocalRootDirPath, COMPort, Indent + 1);
                fileViewItem.OnFileClick += (s, e) => OnFileClick?.Invoke(s, e);
                MainStackPanel.Children.Add(fileViewItem);
            }

            MainButton.Content = MainButtonExpandedContent;
        }

        public void DownloadDirectory(string destDir) {
            Debug.Assert(IsDir);

            AmpyWraper.DownloadDirectoryFromBoard(COMPort, BoardFilePath, destDir);

            DirectoryHasBeenDownloadedOnce = true;
        }

        private void MainButton_Click(object sender, RoutedEventArgs e) {
            if (IsDir) {
                if (IsExpanded)
                    Collapse();
                else
                    Expand();
                IsExpanded = !IsExpanded;
            }
            OnFileClick?.Invoke(this, LocalFilePath);
        }
    }
}