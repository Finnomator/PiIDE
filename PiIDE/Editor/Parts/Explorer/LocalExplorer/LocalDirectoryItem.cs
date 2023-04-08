using FontAwesome.WPF;
using PiIDE.Wrappers;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PiIDE.Editor.Parts.Explorer.LocalExplorer;

public class LocalDirectoryItem : DirectoryItemBase {

    public LocalDirectoryItem(string fullPath, ExplorerBase parentExplorer) : base(fullPath, parentExplorer) {
        Init();

        MenuItem removeFolderMenuItem = new() {
            Header = "Remove folder from workspace",
            Icon = new FontAwesome.WPF.FontAwesome {
                Icon = FontAwesomeIcon.Close,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        removeFolderMenuItem.Click += (_, _) => ((LocalExplorer) ParentExplorer).RemoveFolder(this);
        DirContextMenu.Items.Add(removeFolderMenuItem);
    }

    private LocalDirectoryItem(string fullPath, DirectoryItemBase parentDirectory, ExplorerBase parentExplorer) : base(fullPath, parentDirectory, parentExplorer) => Init();

    private void Init() {
        MenuItem uploadMenuItem = new() {
            Header = "Upload to Pi/",
            Icon = new FontAwesome.WPF.FontAwesome {
                Icon = FontAwesomeIcon.Upload,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        uploadMenuItem.Click += Upload_Click;
        DirContextMenu.Items.Add(uploadMenuItem);
    }

    protected override void Expand() {
        base.Expand();

        Watcher = new(DirectoryPath) {
            NotifyFilter = NotifyFilters.Attributes
                           | NotifyFilters.CreationTime
                           | NotifyFilters.DirectoryName
                           | NotifyFilters.FileName
                           | NotifyFilters.LastAccess
                           | NotifyFilters.LastWrite
                           | NotifyFilters.Security
                           | NotifyFilters.Size,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true,
        };

        Watcher.Created += (_, _) => Dispatcher.Invoke(ReloadContent);
        Watcher.Deleted += Watcher_Deleted;
        Watcher.Renamed += Watcher_Renamed;

        string[] subDirPaths;
        string[] subFilePaths;

        try {
            // TODO: get rid of the catch statement (currently it throws when the parent dir of an open file gets deleted)
            subDirPaths = Directory.GetDirectories(DirectoryPath);
            subFilePaths = Directory.GetFiles(DirectoryPath);
        } catch {
            return;
        }

        foreach (string subDirPath in subDirPaths) {
            LocalDirectoryItem item = new(subDirPath, this, ParentExplorer);
            ChildrenStackPanel.Children.Add(item);
        }

        foreach (string subFilePath in subFilePaths) {
            LocalFileItem item = new(subFilePath, this, ParentExplorer);
            ChildrenStackPanel.Children.Add(item);
        }
    }

    private async void Upload_Click(object sender, RoutedEventArgs e) {
        if (!Tools.EnableBoardInteractions) {
            MessageBox.Show("Unable to connect to Pi", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        SetStatus("Uploading");
        if (await AmpyWrapper.WriteToBoardAsync(GlobalSettings.Default.SelectedCOMPort, DirectoryPath, $"/{DirectoryName}"))
            BasicFileActions.CopyDirectory(DirectoryPath, Path.Combine(GlobalSettings.Default.LocalBoardFilesPath, DirectoryName));
        UnsetStatus();
    }
}