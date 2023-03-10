using PiIDE.Editor.Parts;
using PiIDE.Wrapers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using UserControl = System.Windows.Controls.UserControl;

namespace PiIDE {

    public partial class TextEditorWithFileSelect : UserControl {

        // TODO: Reopen board directory when comport gets changed
        // TODO: Renable RunOnBoardButton when comport gets set or reconnected

        private TextEditor? OpenTextEditor;
        private readonly List<TextEditor> OpenTextEditors = new();
        private const int PylintLinesLimit = 500;

        public static string LocalBoardPath => GlobalSettings.Default.LocalBoardFilesPath;

        public TextEditorWithFileSelect() {
            InitializeComponent();

            MessagesWindow.SelectionChanged += MessagesWindow_SelectionChanged;

            if (!Directory.Exists(LocalBoardPath))
                Directory.CreateDirectory(LocalBoardPath);

            string[] lastOpenedLocalFiles = GlobalSettings.Default.LastOpenedLocalFilePaths.ToArray();
            string[] lastOpenedBoardFiles = GlobalSettings.Default.LastOpenedBoardFilePaths.ToArray();
            string? lastOpenedFile = GlobalSettings.Default.LastOpenedFilePath;

            foreach (string path in lastOpenedLocalFiles)
                OpenFile(path, path != lastOpenedFile, false);

            foreach (string path in lastOpenedBoardFiles)
                OpenFile(path, path != lastOpenedFile, true);

            OpenDirectory(GlobalSettings.Default.OpenDirectoryPath);
            OpenBoardDirectory();

            if (Tools.EnableBoardInteractions)
                EnableBoardInteractions();
            else
                DisableBoardInteractions();
        }

        private void MessagesWindow_SelectionChanged(object? _, PylintMessage e) => GoToPylintMessage(e);

        public void OpenFile(string filePath, bool openInBackground, bool onBoard) {

            if (IsFileOpen(filePath)) {
                MainTabControl.SelectedIndex = GetTabIndexOfOpenFile(filePath);
                return;
            }

            if (!File.Exists(filePath)) {
                RemoveFileFromSettings(filePath);
                return;
            }

            AddFile(filePath, openInBackground, onBoard);
            if (!openInBackground)
                MainTabControl.SelectedIndex = MainTabControl.Items.Count - 1;
        }

        private static void RemoveFileFromSettings(string path) {
            if (GlobalSettings.Default.LastOpenedLocalFilePaths.Contains(path))
                GlobalSettings.Default.LastOpenedLocalFilePaths.Remove(path);

            if (GlobalSettings.Default.LastOpenedBoardFilePaths.Contains(path))
                GlobalSettings.Default.LastOpenedBoardFilePaths.Remove(path);
        }

        public bool IsFileOpen(string filePath) => GetTabIndexOfOpenFile(filePath) != -1;

        private TextEditor AddFile(string filePath, bool openInBackground = false, bool onBoard = false, int atIndex = -1) {
            TextEditor textEditor;
            EditorTabItem tabItem;

            if (onBoard) {
                textEditor = new BoardTextEditor(filePath, filePath[LocalBoardPath.Length..], openInBackground) { DisableAllWrapers = openInBackground };
                ((BoardTextEditor) textEditor).StartedWritingToBoard += (s, e) => { UploadingFileStatusStackPanel.Visibility = Visibility.Visible; };
                ((BoardTextEditor) textEditor).DoneWritingToBoard += (s, e) => { UploadingFileStatusStackPanel.Visibility = Visibility.Collapsed; };
                ((BoardTextEditor) textEditor).StartedPythonExecutionOnBoard += (s, e) => OutputTabControl.SelectedIndex = 1;

                tabItem = new BoardEditorTabItem(filePath) {
                    Content = textEditor,
                };
            } else {
                textEditor = new(filePath, openInBackground);
                tabItem = new(filePath) {
                    Content = textEditor,
                };
            }

            textEditor.ContentChanged += delegate {
                tabItem.SaveLocalButton.IsEnabled = !textEditor.ContentIsSaved;
            };

            textEditor.OnFileSaved += delegate {
                tabItem.SaveLocalButton.IsEnabled = false;
                if (GlobalSettings.Default.PylintIsUsable)
                    UpdatePylintMessages(textEditor);
            };

            textEditor.StartedPythonExecution += (s, e) => OutputTabControl.SelectedIndex = 2;

            tabItem.CloseTabClick += (s, filePath) => {
                TextEditor? editor = GetEditorFromPath(filePath);
                if (editor is null)
                    return;

                if (!editor.ContentIsSaved) {
                    MessageBoxResult msgbr = MessageBox.Show("This file is not saved. Do you want to close it anyway?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (msgbr == MessageBoxResult.Yes)
                        CloseFile(filePath);
                } else
                    CloseFile(filePath);
            };

            tabItem.SaveLocalClick += (s, filePath) => textEditor.SaveFile(true);

            if (atIndex < 0)
                MainTabControl.Items.Add(tabItem);
            else
                MainTabControl.Items.Insert(atIndex, tabItem);

            OpenTextEditors.Add(textEditor);

            if (onBoard) {
                if (!GlobalSettings.Default.LastOpenedBoardFilePaths.Contains(filePath))
                    GlobalSettings.Default.LastOpenedBoardFilePaths.Add(filePath);
            } else {
                if (!GlobalSettings.Default.LastOpenedLocalFilePaths.Contains(filePath))
                    GlobalSettings.Default.LastOpenedLocalFilePaths.Add(filePath);
            }

            return textEditor;
        }

        private async void UpdatePylintMessages(TextEditor textEditor) {
            IEnumerable<TextEditor> pythonEditorsBelowLineLimit = OpenTextEditors.Where(x => x.IsPythonFile && x.EditorText.CountLines() < PylintLinesLimit);
            PylintMessage[] pylintMessages = await MessagesWindow.UpdateLintMessages(pythonEditorsBelowLineLimit.Select(x => x.FilePath).ToArray());
            textEditor.UpdatePylint(pylintMessages.Where(x => Path.GetFullPath(x.Path) == Path.GetFullPath(textEditor.FilePath)).ToArray());
        }

        private TextEditor? GetEditorFromPath(string path) => OpenTextEditors.Find(x => x.FilePath == path);

        public void AddTempFile() {
            string newFilePath = $"TempFiles/temp_file{Directory.GetFiles("TempFiles").Length + 1}.py";
            File.Create(newFilePath).Close();
            OpenFile(newFilePath, false, false);
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (MainTabControl.SelectedItem is null)
                return;

            if (OpenTextEditor is not null)
                OpenTextEditor.DisableAllWrapers = true;

            OpenTextEditor = (TextEditor) ((TabItem) MainTabControl.SelectedItem).Content;
            OpenTextEditor.DisableAllWrapers = false;
            GlobalSettings.Default.LastOpenedFilePath = OpenTextEditor.FilePath;
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == true)
                OpenFile(openFileDialog.FileName, false, false);
        }

        private void OpenDirectoryButton_Click(object sender, RoutedEventArgs e) {
            using FolderBrowserDialog fbd = new();
            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                OpenDirectory(fbd.SelectedPath);
            }
        }

        private void CreateNewFileButton_Click(object sender, RoutedEventArgs e) => AddTempFile();

        public void OpenDirectory(string directory) {
            RootPathTextBox.Text = Path.GetFullPath(directory);
            LocalExplorer = new(directory);
            LocalExplorer.OnFileClick += (s) => {
                OpenFile(s.FilePath, false, false);
            };
            LocalExplorer.OnFileDeleted += (s, filePath) => {
                CloseFile(filePath);
            };
            LocalExplorer.OnFileRenamed += (s, oldPath, newPath) => {
                if (File.Exists(newPath)) {
                    OpenRenamedFile(oldPath, newPath);
                }
            };
            LocalDirectoryScrollViewer.Content = LocalExplorer;
            GlobalSettings.Default.OpenDirectoryPath = directory;
        }

        private void OpenRenamedFile(string oldPath, string newPath) {
            if (IsFileOpen(oldPath)) {
                int index = GetTabIndexOfOpenFile(oldPath);
                MainTabControl.Items.RemoveAt(index);
                AddFile(newPath, atIndex: index);
            }
        }

        private void CloseFile(string filePath) {
            if (!IsFileOpen(filePath))
                return;

            MainTabControl.Items.RemoveAt(GetTabIndexOfOpenFile(filePath));
            TextEditor? editor = GetEditorFromPath(filePath);
            if (editor is not null)
                OpenTextEditors.Remove(editor);

            RemoveFileFromSettings(filePath);
        }

        public void OpenBoardDirectory(string directory = "") {
            // "" is the default path, do not use "/"!

            BoardExplorer = new(LocalBoardPath, directory);

            BoardExplorer.OnFileClick += (s) => {
                OpenFile(s.FilePath, false, true);
            };

            BoardExplorer.OnFileDeleted += (s, filePath) => {
                CloseFile(filePath);
            };

            BoardExplorer.OnFileRenamed += (s, oldPath, newPath) => {
                if (File.Exists(newPath)) {
                    OpenRenamedFile(oldPath, newPath);
                }
            };

            BoardDirectoryScrollViewer.Content = BoardExplorer;
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e) {

            if (!Tools.EnableBoardInteractions) {
                DisableBoardInteractions();
                return;
            }

            SyncButton.IsEnabled = false;

            SyncOptionsWindow syncWindow = new();
            Point mousePos = PointToScreen(Mouse.GetPosition(this));
            syncWindow.Left = mousePos.X;
            syncWindow.Top = mousePos.Y;
            syncWindow.Show();
            syncWindow.Focus();

            syncWindow.Closed += async delegate {

                switch (syncWindow.SyncOptionResult) {
                    case SyncOptionResult.Cancel:
                        break;
                    case SyncOptionResult.OverwriteAllLocalFiles:
                        SyncStatus.Visibility = Visibility.Visible;

                        await AmpyWraper.DownloadDirectoryFromBoardAsync(GlobalSettings.Default.SelectedCOMPort, BoardExplorer.DirectoryPathOnBoard, BoardExplorer.DirectoryPath);

                        SyncStatus.Visibility = Visibility.Collapsed;

                        for (int i = 0; i < OpenTextEditors.Count; i++)
                            if (OpenTextEditors[i] is BoardTextEditor)
                                OpenTextEditors[i].ReloadFile();
                        break;
                }

                SyncButton.IsEnabled = true;
            };
        }

        private void GoToPylintMessage(PylintMessage pylintMessage) => GoTo(pylintMessage.Path, pylintMessage.Line, pylintMessage.Column);

        private void GoTo(string filePath, int row, int column) {
            OpenFile(filePath, false, false);
            if (OpenTextEditor is null)
                throw new NullReferenceException();
            if (!OpenTextEditor.ContentLoaded)
                OpenTextEditor.ReloadFile();
            OpenTextEditor.SetCaretPosition(row, column);
            OpenTextEditor.ScrollToPosition(row, column);
        }

        private int GetTabIndexOfOpenFile(string filePath) {
            for (int i = 0; i < MainTabControl.Items.Count; i++) {
                TabItem tabItem = (TabItem) MainTabControl.Items[i];
                TextEditor content = (TextEditor) tabItem.Content;
                if (Path.GetFullPath(content.FilePath) == Path.GetFullPath(filePath))
                    return i;
            }
            return -1;
        }

        private void DisableBoardInteractions() => SyncButton.IsEnabled = false;

        private void EnableBoardInteractions() => SyncButton.IsEnabled = true;

        private void ConnectToBoardButton_Click(object sender, RoutedEventArgs e) {
            if (Tools.EnableBoardInteractions)
                EnableBoardInteractions();
            else {
                MessageBox.Show("Unable to connect to board, did you select the correct COM port?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                DisableBoardInteractions();
            }
        }

        public bool AreAllFilesSaved() {
            for (int i = 0; i < OpenTextEditors.Count; ++i) {
                if (!OpenTextEditors[i].ContentIsSaved)
                    return false;
            }
            return true;
        }
    }
}
