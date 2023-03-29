using System.Collections.Generic;
using System.Windows.Controls;

namespace PiIDE.Editor.Parts.Explorer {

    public abstract partial class ExplorerBase : UserControl {

        public delegate void FileDeletedEventHandler(DirectoryItemBase sender, string deletedFilePath);
        public event FileDeletedEventHandler? FileDeleted;
        public void OnFileDeleted(DirectoryItemBase sender, string deletedFilePath) => FileDeleted?.Invoke(sender, deletedFilePath);

        public delegate void FileRenamedEventHandler(DirectoryItemBase sender, string oldFilePath, string newFilePath);
        public event FileRenamedEventHandler? FileRenamed;
        public void OnFileRenamed(DirectoryItemBase sender, string oldFilePath, string newFilepath) => FileRenamed?.Invoke(sender, oldFilePath, newFilepath);

        public delegate void FileClickEventHandler(FileItemBase sender);
        public event FileClickEventHandler? FileClick;
        public void OnFileClick(FileItemBase fileItem) => FileClick?.Invoke(fileItem);

        public ExplorerBase() {
            InitializeComponent();
        }

        public void AddFolder(DirectoryItemBase folder) {
            DirectoryItemsWrapPanel.Children.Add(folder);
        }

        public void ClearFolders() {
            DirectoryItemsWrapPanel.Children.Clear();
        }

        public void RemoveFolder(DirectoryItemBase folder) {
            DirectoryItemsWrapPanel.Children.Remove(folder);
        }
    }
}
