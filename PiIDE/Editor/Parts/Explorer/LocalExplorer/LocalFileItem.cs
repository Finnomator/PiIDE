using System.Windows;

namespace PiIDE.Editor.Parts.Explorer.LocalExplorer {
    public class LocalFileItem : FileItemBase {

        public LocalFileItem(string fullPath, LocalDirectoryItem parentDirectory) : base(fullPath, parentDirectory) {

        }

        protected override void Copy_Click(object sender, RoutedEventArgs e) => FileCopier.Copy(FilePath, false);

        protected override void Cut_Click(object sender, RoutedEventArgs e) => FileCopier.Cut(FilePath, false);

        protected override void Delete_Click(object sender, RoutedEventArgs e) => BasicFileActions.DeleteFile(FilePath);

        protected override void Paste_Click(object sender, RoutedEventArgs e) => FileCopier.Paste(ParentDirectory.DirectoryPath);

        protected override void RenameFile(string oldPath, string newPath, string newName) => BasicFileActions.RenameFile(oldPath, newName);
    }
}
