﻿using FontAwesome.WPF;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PiIDE.Editor.Parts.Explorer;

public abstract partial class DirectoryItemBase {

    public string DirectoryPath { get; private set; }
    public int Indent { get; }

    private bool IsExpanded;

    protected readonly ExplorerBase ParentExplorer;
    protected readonly DirectoryItemBase? ParentDirectory;
    protected string DirectoryName;
    protected FileSystemWatcher? Watcher;

    protected readonly string DirectoryNameForTextBlock;
    private readonly FontAwesome.WPF.FontAwesome FolderOpenIcon = new() { Icon = FontAwesomeIcon.FolderOpen };
    private readonly FontAwesome.WPF.FontAwesome FolderClosedIcon = new() { Icon = FontAwesomeIcon.Folder };
    private static readonly RotateTransform NinetyDegreeTurn = new(90);

    protected DirectoryItemBase(string fullPath, ExplorerBase parentExplorer) {
        InitializeComponent();

        DirectoryPath = fullPath;
        ParentExplorer = parentExplorer;

        DirectoryName = Path.GetFileName(DirectoryPath);

        IndentColumn.Width = new GridLength(Indent * 10);

        DirectoryNameForTextBlock = DirectoryName;
        if (string.IsNullOrEmpty(DirectoryNameForTextBlock))
            DirectoryNameForTextBlock = DirectoryPath;

        FileNameTextBlock.Text = DirectoryNameForTextBlock;
        ToolTip = DirectoryPath;
    }

    protected DirectoryItemBase(string fullPath, DirectoryItemBase parentDirectory, ExplorerBase parentExplorer) {
        InitializeComponent();

        DirectoryPath = fullPath;
        ParentExplorer = parentExplorer;
        ParentDirectory = parentDirectory;

        DirectoryName = Path.GetFileName(DirectoryPath);

        Indent = parentDirectory.Indent + 1;
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

    protected void Collapse() {
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

    protected void Watcher_Renamed(object sender, RenamedEventArgs e) => Dispatcher.Invoke(() => {
        ParentExplorer.OnFileRenamed(this, e.OldFullPath, e.FullPath);
        ReloadContent();
    });

    protected void Watcher_Deleted(object sender, FileSystemEventArgs e) => Dispatcher.Invoke(() => {
        ParentExplorer.OnFileDeleted(this, e.FullPath);
        try {
            ReloadContent();
        } catch (ArgumentException) { }
    });

    private void MainButton_Click(object sender, RoutedEventArgs e) {
        if (IsExpanded)
            Collapse();
        else
            Expand();
        IsExpanded = !IsExpanded;
    }

    protected virtual void RenameDirectory(string oldPath, string newName) => BasicFileActions.RenameDirectory(oldPath, newName);

    private void Copy_Click(object sender, RoutedEventArgs e) => FileCopier.Copy(DirectoryPath, true);

    private void Cut_Click(object sender, RoutedEventArgs e) => FileCopier.Cut(DirectoryPath, true);

    protected virtual void Delete_Click(object sender, RoutedEventArgs e) => BasicFileActions.DeleteDirectory(DirectoryPath);

    public virtual void Paste_Click(object sender, RoutedEventArgs e) => FileCopier.Paste(DirectoryPath);

    protected virtual void Rename_Click(object sender, RoutedEventArgs e) {
        RenameTextBox.Visibility = Visibility.Visible;
        RenameTextBox.Text = DirectoryNameForTextBlock;
        RenameTextBox.SelectAll();
        RenameTextBox.Focus();
    }

    protected virtual void AddFile_Click(object sender, RoutedEventArgs e) => Tools.TryCreateFile(Path.Combine(DirectoryPath, "new_file.py"));

    protected virtual void AddFolder_Click(object sender, RoutedEventArgs e) => Tools.TryCreateDirectory(Path.Combine(DirectoryPath, "NewFolder"));

    private void OpenInExplorer_Click(object sender, RoutedEventArgs e) => Process.Start(new ProcessStartInfo {
        WorkingDirectory = DirectoryPath,
        Arguments = ".",
        FileName = "explorer",
    });

    private void OpenInCmd_Click(object sender, RoutedEventArgs e) => Process.Start(new ProcessStartInfo {
        WorkingDirectory = DirectoryPath,
        FileName = "cmd",
    });

    protected void SetStatus(string status) {
        StatusTextBlock.Text = status;
        Status.Visibility = Visibility.Visible;
    }

    protected void UnsetStatus() => Status.Visibility = Visibility.Collapsed;

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

        RenameTextBox.Visibility = Visibility.Collapsed;

        string oldName = DirectoryName;
        string newName = textBox.Text;
        string newPath = Path.Combine(DirectoryPath[^newName.Length..], newName);

        if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) {
            MessageBox.Show("Invalid characters in path", "Renaming Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (newName == oldName)
            return;

        if (Directory.Exists(newPath)) {
            MessageBox.Show("The directory already exists", "Renaming Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        DirectoryName = newName;
        FileNameTextBlock.Text = newName;
        RenameDirectory(DirectoryPath, newName);
        DirectoryPath = newPath;
    }
}