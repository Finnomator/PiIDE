namespace PiIDE.Editor.Parts.Explorer.LocalExplorer {
    internal class LocalExplorer : ExplorerBase {

        public LocalExplorer() => Header.Content = "Local Files";

        public void AddFolder(string path) {
            AddFolder(new LocalDirectoryItem(path, this));

            if (!GlobalSettings.Default.LastOpenedLocalFolderPaths.Contains(path))
                GlobalSettings.Default.LastOpenedLocalFolderPaths.Add(path);
        }

        public void RemoveFolder(LocalDirectoryItem directoryItem) {
            base.RemoveFolder(directoryItem);
            if (GlobalSettings.Default.LastOpenedLocalFolderPaths.Contains(directoryItem.DirectoryPath))
                GlobalSettings.Default.LastOpenedLocalFolderPaths.Remove(directoryItem.DirectoryPath);
        }
    }
}
