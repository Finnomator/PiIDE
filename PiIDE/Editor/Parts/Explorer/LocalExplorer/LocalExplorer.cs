namespace PiIDE.Editor.Parts.Explorer.LocalExplorer {
    internal class LocalExplorer : LocalDirectoryItem {

        public LocalExplorer() : base("C:\\", null) {

        }

        public LocalExplorer(string fullPath) : base(fullPath, null) {

        }
    }
}
