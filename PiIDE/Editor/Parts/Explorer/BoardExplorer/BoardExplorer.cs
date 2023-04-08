namespace PiIDE.Editor.Parts.Explorer.BoardExplorer {
    public class BoardExplorer : ExplorerBase {

        public BoardExplorer() {
            Header.Content = "Board Files";
            BoardDirectoryItem folderItem = new(GlobalSettings.Default.LocalBoardFilesPath, "", this) {
                FileNameTextBlock =
                    {
                        Text = "Pi"
                    }
            };
            base.AddFolder(folderItem);
        }

        public new static void AddFolder(DirectoryItemBase _) => throw new System.Exception("This feature should not be used");

        public new static void ClearFolders() => throw new System.Exception("This feature should not be used");

        public new static void RemoveFolder(DirectoryItemBase _) => throw new System.Exception("This feature should not be used");
    }
}
