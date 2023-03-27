﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PiIDE.Editor.Parts.Explorer {

    public abstract partial class DirectoryItemBase : UserControl {

        public string DirectoryPath { get; private set; }
        public int Indent { get; private set; }

        public delegate void FileDeletedEventHandler(DirectoryItemBase sender, string deletedFilePath);
        public virtual event FileDeletedEventHandler? FileDeleted;
        protected void OnFileDeleted(DirectoryItemBase sender, string deletedFilePath) => FileDeleted?.Invoke(sender, deletedFilePath);

        public delegate void FileRenamedEventHandler(DirectoryItemBase sender, string oldFilePath, string newFilePath);
        public virtual event FileRenamedEventHandler? FileRenamed;
        protected void OnFileRenamed(DirectoryItemBase sender, string oldFilePath, string newFilepath) => FileRenamed?.Invoke(sender, oldFilePath, newFilepath);

        public delegate void FileClickEventHandler(FileItemBase sender);
        public virtual event FileClickEventHandler? FileClick;
        protected void OnFileClick(FileItemBase fileItem) => FileClick?.Invoke(fileItem);

        private bool IsExpanded;

        protected readonly DirectoryItemBase? ParentDirectory;
        protected string DirectoryName;
        protected FileSystemWatcher? Watcher;

        protected readonly string DirectoryNameForTextBlock;
        private readonly FontAwesome.WPF.FontAwesome FolderOpenIcon = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.FolderOpen };
        private readonly FontAwesome.WPF.FontAwesome FolderClosedIcon = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.Folder };
        private static readonly RotateTransform NinetyDegreeTurn = new RotateTransform(90);

        public DirectoryItemBase(string fullPath, DirectoryItemBase? parentDirectory) {
            InitializeComponent();

            DirectoryPath = fullPath;
            ParentDirectory = parentDirectory;

            DirectoryName = Path.GetFileName(DirectoryPath);

            Indent = ParentDirectory == null ? 0 : ParentDirectory.Indent + 1;
            IndentColumn.Width = new GridLength(Indent * 10);

            DirectoryNameForTextBlock = DirectoryName;
            if (string.IsNullOrEmpty(DirectoryNameForTextBlock))
                DirectoryNameForTextBlock = DirectoryPath;

            FileNameTextBlock.Text = DirectoryNameForTextBlock;
        }

        protected virtual void Expand() {
            IsExpandedTextBlock.RenderTransform = NinetyDegreeTurn;
            FileIconControl.Content = FolderOpenIcon;
        }

        protected virtual void Collapse() {
            ChildrenStackPanel.Children.Clear();
            IsExpandedTextBlock.RenderTransform = null;
            FileIconControl.Content = FolderClosedIcon;

            if (Watcher != null) {
                Watcher.Dispose();
                Watcher = null;
            }
        }

        protected void ReloadContent() {
            ChildrenStackPanel.Children.Clear();
            Expand();
        }

        protected void Watcher_Renamed(object sender, RenamedEventArgs e) {
            Dispatcher.Invoke(() => {
                FileRenamed?.Invoke(this, e.OldFullPath, e.FullPath);
                ReloadContent();
            });
        }

        protected void Watcher_Deleted(object sender, FileSystemEventArgs e) {
            Dispatcher.Invoke(() => {
                FileDeleted?.Invoke(this, e.FullPath);
                try {
                    ReloadContent();
                } catch (ArgumentException) { }
            });
        }

        private void MainButton_Click(object sender, RoutedEventArgs e) {
            if (IsExpanded)
                Collapse();
            else
                Expand();
            IsExpanded = !IsExpanded;
        }

        protected virtual void RenameDirectory(string oldPath, string newPath, string newName) => BasicFileActions.RenameDirectory(oldPath, newName);

        protected virtual void Copy_Click(object sender, RoutedEventArgs e) => FileCopier.Copy(DirectoryPath, true);

        protected virtual void Cut_Click(object sender, RoutedEventArgs e) => FileCopier.Cut(DirectoryPath, true);

        protected virtual void Delete_Click(object sender, RoutedEventArgs e) => BasicFileActions.DeleteDirectory(DirectoryPath);

        protected virtual void Paste_Click(object sender, RoutedEventArgs e) => FileCopier.Paste(DirectoryPath);

        protected virtual void Rename_Click(object sender, RoutedEventArgs e) {
            RenameTextBox.Visibility = Visibility.Visible;
            RenameTextBox.Text = DirectoryNameForTextBlock;
            RenameTextBox.SelectAll();
            RenameTextBox.Focus();
        }

        protected void SetStatus(string status) {
            StatusTextBlock.Text = status;
            Status.Visibility = Visibility.Visible;
        }

        protected void UnsetStatus() {
            Status.Visibility = Visibility.Collapsed;
        }

        private void RenameTextBox_KeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Enter:
                    RenameFromTextBox(RenameTextBox);
                    break;
                case Key.Escape:
                    RenameTextBox.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void RenameTextBox_LostFocus(object sender, RoutedEventArgs e) => RenameTextBox.Visibility = Visibility.Collapsed;

        private void RenameFromTextBox(TextBox textBox) {

            string oldName = DirectoryName;
            string newName = textBox.Text;
            string newPath = Path.Combine(DirectoryPath[^newName.Length..], newName);

            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) {
                MessageBox.Show("Invalid characters in path", "Renaming Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RenameTextBox.Visibility = Visibility.Collapsed;
                return;
            }

            if (newName == oldName) {
                RenameTextBox.Visibility = Visibility.Collapsed;
                return;
            }

            if (Directory.Exists(newPath)) {
                MessageBox.Show("The directory already exists", "Renaming Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RenameTextBox.Visibility = Visibility.Collapsed;
                return;
            }

            DirectoryName = newName;
            FileNameTextBlock.Text = DirectoryName;
            RenameDirectory(DirectoryPath, newPath, DirectoryName);
            DirectoryPath = newPath;
        }
    }
}
