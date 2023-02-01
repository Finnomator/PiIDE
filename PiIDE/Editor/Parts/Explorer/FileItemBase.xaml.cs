using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PiIDE.Editor.Parts.Explorer {

    public abstract partial class FileItemBase : UserControl {

        public string FilePath { get; private set; }

        public delegate void OnClickEventHandler(FileItemBase sender);
        public event OnClickEventHandler? OnClick;

        protected readonly DirectoryItemBase ParentDirectory;
        protected string FileName;
        protected readonly string FileNameForTextBlock;

        public FileItemBase(string fullPath, DirectoryItemBase parentDirectory) {
            InitializeComponent();

            FilePath = fullPath;
            ParentDirectory = parentDirectory;

            int indent = parentDirectory.Indent + 1;
            IndentColumn.Width = new GridLength(indent * 10);

            FileName = Path.GetFileName(FilePath);

            FileNameForTextBlock = FileName;
            if (string.IsNullOrEmpty(FileNameForTextBlock))
                FileNameForTextBlock = FilePath;

            FileNameTextBlock.Text = FileNameForTextBlock;
        }

        private void MainButton_Click(object sender, RoutedEventArgs e) => OnClick?.Invoke(this);

        protected abstract void Copy_Click(object sender, RoutedEventArgs e);
        protected abstract void Cut_Click(object sender, RoutedEventArgs e);
        protected abstract void Paste_Click(object sender, RoutedEventArgs e);
        protected abstract void Delete_Click(object sender, RoutedEventArgs e);

        protected abstract void RenameFile(string oldPath, string newPath, string newName);

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
}
