namespace PiIDE.Editor.Parts.Explorer.BoardExplorer {
    public class BoardExplorer : BoardDirectoryItem {

        public BoardExplorer() : base("BoardFiles", "", null) {
            // TODO: replace "Pi" with something dynamic
            FileNameTextBlock.Text = "Pi";
            MainButton.ContextMenu = null;
        }

        public BoardExplorer(string fullPath, string directoryPathOnBoard) : base(fullPath, directoryPathOnBoard, null) {
            FileNameTextBlock.Text = "Pi";
            if (directoryPathOnBoard != "")
                FileNameTextBlock.Text += $"/{directoryPathOnBoard}";
            MainButton.ContextMenu = null;
        }
    }
}
