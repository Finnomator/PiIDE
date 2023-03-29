namespace PiIDE.Editor.Parts.Explorer.LocalExplorer {
    internal class LocalExplorer : ExplorerBase {
        public LocalExplorer() {
            Header.Content = "Local Files";
        }
        public void AddFolder(string path) => AddFolder(new LocalDirectoryItem(path, this));
    }
}
