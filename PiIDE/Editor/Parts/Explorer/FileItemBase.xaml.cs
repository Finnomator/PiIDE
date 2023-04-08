using PiIDE.Assets.Icons;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PiIDE.Editor.Parts.Explorer;

public abstract partial class FileItemBase {

    public string FilePath { get; private set; }

    public delegate void OnClickEventHandler(FileItemBase sender);
    public event OnClickEventHandler? OnClick;

    protected readonly DirectoryItemBase ParentDirectory;
    protected string FileName;
    protected readonly string FileNameForTextBlock;

    protected FileItemBase(string fullPath, DirectoryItemBase parentDirectory, ExplorerBase explorer) {
        InitializeComponent();

        FilePath = fullPath;
        ParentDirectory = parentDirectory;

        int indent = parentDirectory.Indent + 1;
        IndentColumn.Width = new GridLength(indent * 10);

        FileName = Path.GetFileName(FilePath);

        OnClick += _ => explorer.OnFileClick(this);

        FileNameForTextBlock = FileName;
        if (string.IsNullOrEmpty(FileNameForTextBlock))
            FileNameForTextBlock = FilePath;

        FileNameTextBlock.Text = FileNameForTextBlock;

        FileIconControl.Content = new Image {
            Source = Icons.GetFileIcon(FilePath),
            Width = FileIconControl.Width - 4,
        };
    }

    private void MainButton_Click(object sender, RoutedEventArgs e) => OnClick?.Invoke(this);

    protected void SetStatus(string status) {
        Status.Visibility = Visibility.Visible;
        StatusTextBlock.Text = status;
    }

    protected void UnsetStatus() => Status.Visibility = Visibility.Collapsed;

    protected virtual void Copy_Click(object sender, RoutedEventArgs e) => FileCopier.Copy(FilePath, false);
    protected virtual void Cut_Click(object sender, RoutedEventArgs e) => FileCopier.Cut(FilePath, false);
    protected virtual void Delete_Click(object sender, RoutedEventArgs e) => BasicFileActions.DeleteFile(FilePath);
    protected virtual void Paste_Click(object sender, RoutedEventArgs e) => FileCopier.Paste(ParentDirectory.DirectoryPath);
    protected virtual void RenameFile(string oldPath, string newPath, string newName) => BasicFileActions.RenameFile(oldPath, newName);

    protected virtual void Rename_Click(object sender, RoutedEventArgs e) {
        RenameTextBox.Visibility = Visibility.Visible;
        RenameTextBox.Text = FileNameForTextBlock;
        RenameTextBox.SelectAll();
        RenameTextBox.Focus();
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

        string oldName = FileName;
        string newName = textBox.Text;
        string newPath = Path.Combine(FilePath[^newName.Length..], newName);

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
            MessageBox.Show("The file already exists", "Renaming Error", MessageBoxButton.OK, MessageBoxImage.Error);
            RenameTextBox.Visibility = Visibility.Collapsed;
            return;
        }

        FileName = newName;
        FileNameTextBlock.Text = FileName;
        RenameFile(FilePath, newPath, newName);
        FilePath = newPath;
    }
}