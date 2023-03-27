namespace PiIDE.Editor.Parts.Explorer.LocalExplorer {
    internal class LocalExplorer : LocalDirectoryItem {

        public LocalExplorer() : base("C:\\", null) {
            MainButton.ContextMenu = null;
        }

        public LocalExplorer(string fullPath) : base(fullPath, null) {
            MainButton.ContextMenu = null;
        }
    }
}
