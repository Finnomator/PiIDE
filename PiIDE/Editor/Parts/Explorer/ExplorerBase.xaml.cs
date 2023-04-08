namespace PiIDE.Editor.Parts.Explorer;

public abstract partial class ExplorerBase {

    public delegate void FileDeletedEventHandler(DirectoryItemBase sender, string deletedFilePath);
    public event FileDeletedEventHandler? FileDeleted;
    public void OnFileDeleted(DirectoryItemBase sender, string deletedFilePath) => FileDeleted?.Invoke(sender, deletedFilePath);

    public delegate void FileRenamedEventHandler(DirectoryItemBase sender, string oldFilePath, string newFilePath);
    public event FileRenamedEventHandler? FileRenamed;
    public void OnFileRenamed(DirectoryItemBase sender, string oldFilePath, string newFilepath) => FileRenamed?.Invoke(sender, oldFilePath, newFilepath);

    public delegate void FileClickEventHandler(FileItemBase sender);
    public event FileClickEventHandler? FileClick;
    public void OnFileClick(FileItemBase fileItem) => FileClick?.Invoke(fileItem);

    protected ExplorerBase() => InitializeComponent();

    protected void AddFolder(DirectoryItemBase folder) => DirectoryItemsWrapPanel.Children.Add(folder);

    protected void ClearFolders() => DirectoryItemsWrapPanel.Children.Clear();

    protected void RemoveFolder(DirectoryItemBase folder) => DirectoryItemsWrapPanel.Children.Remove(folder);
}