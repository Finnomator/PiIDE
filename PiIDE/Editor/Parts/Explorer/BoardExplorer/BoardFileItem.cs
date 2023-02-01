using PiIDE.Editor.Parts.Explorer.LocalExplorer;
using System.Windows;

namespace PiIDE.Editor.Parts.Explorer.BoardExplorer {
    public class BoardFileItem : LocalFileItem {

        public string FilePathOnBoard { get; private set; }

        public BoardFileItem(string fullLocalPath, string pathOnBoard, BoardDirectoryItem parentDirectory) : base(fullLocalPath, parentDirectory) {
            FilePathOnBoard = pathOnBoard;
        }

        // TODO: Implement these features
        protected override void Copy_Click(object sender, RoutedEventArgs e) => ErrorMessager.FeatureNotSupported();

        protected override void Cut_Click(object sender, RoutedEventArgs e) => ErrorMessager.FeatureNotSupported();

        protected override void Delete_Click(object sender, RoutedEventArgs e) => ErrorMessager.FeatureNotSupported();

        protected override void Paste_Click(object sender, RoutedEventArgs e) => ErrorMessager.FeatureNotSupported();

        protected override void RenameFile(string oldPath, string newPath, string newName) => ErrorMessager.FeatureNotSupported();

        // TODO: this method should not be overriden, but as long as the feature is not supported, it will
        protected override void Rename_Click(object sender, RoutedEventArgs e) => ErrorMessager.FeatureNotSupported();
    }
}
