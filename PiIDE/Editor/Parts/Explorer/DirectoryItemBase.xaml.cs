using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace PiIDE.Editor.Parts.Explorer {

    public abstract partial class DirectoryItemBase : UserControl {

        public string DirectoryPath { get; private set; }
        public int Indent { get; private set; }

        public delegate void FileDeletedEventHandler(DirectoryItemBase sender, string deletedFilePath);
        public abstract event FileDeletedEventHandler? OnFileDeleted;

        public delegate void FileRenamedEventHandler(DirectoryItemBase sender, string oldFilePath, string newFilePath);
        public abstract event FileRenamedEventHandler? OnFileRenamed;

        public delegate void FileClickEventHandler(FileItemBase sender);
        public abstract event FileClickEventHandler? OnFileClick;

        private bool IsExpanded;

        protected readonly DirectoryItemBase? ParentDirectory;
        protected string DirectoryName;
        protected readonly string DirectoryNameForTextBlock;

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

        protected abstract void Expand();
        protected abstract void Collapse();
        protected abstract void RenameDirectory(string oldPath, string newPath, string newName);

        protected void ReloadContent() {
            ChildrenStackPanel.Children.Clear();
            Expand();
        }

        private void MainButton_Click(object sender, RoutedEventArgs e) {
            if (IsExpanded)
                Collapse();
            else
                Expand();
            IsExpanded = !IsExpanded;
        }

        protected abstract void Copy_Click(object sender, RoutedEventArgs e);
        protected abstract void Cut_Click(object sender, RoutedEventArgs e);
        protected abstract void Paste_Click(object sender, RoutedEventArgs e);
        protected abstract void Delete_Click(object sender, RoutedEventArgs e);

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
