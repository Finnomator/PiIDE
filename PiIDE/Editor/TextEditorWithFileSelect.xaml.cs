using PiIDE.Editor.Parts;
using PiIDE.Wrapers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using UserControl = System.Windows.Controls.UserControl;

namespace PiIDE {

    public partial class TextEditorWithFileSelect : UserControl {

        // TODO: Reopen board directory when comport gets changed
        // TODO: Renable RunOnBoardButton when comport gets set or reconnected

        private TextEditor OpenTextEditor;
        private readonly List<string> PythonOnlyFilePaths = new();
        private readonly List<TextEditor> OpenTextEditors = new();

        public const string LocalBoardPath = "BoardFiles/";

        public TextEditorWithFileSelect() {
            InitializeComponent();
            OpenTextEditor = (TextEditor) DefaultEditor.Content;

            MainTabControl.Items.Clear();

            MessagesWindow.SelectionChanged += MessagesWindow_SelectionChanged;

            AmpyWraper.AmpyExited += Ampy_Exited;
            PythonWraper.PythonExited += Python_Exited;

            if (!Directory.Exists(LocalBoardPath))
                Directory.CreateDirectory(LocalBoardPath);

            OpenFile("TempFiles/temp_file1.py");
            OpenDirectory(GlobalSettings.Default.OpenDirectoryPath);
            OpenBoardDirectory();
            RunFileLocalButton.IsEnabled = GlobalSettings.Default.PythonIsInstalled;

            if (Tools.EnableBoardInteractions)
                EnableBoardInteractions();
            else
                DisableBoardInteractions();
        }

        private void Ampy_Exited(object? sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                RunFileOnBoardButton.IsEnabled = true;
            });
        }

        private void Python_Exited(object? sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                RunFileLocalButton.IsEnabled = true;
            });
        }

        private void MessagesWindow_SelectionChanged(object? _, PylintMessage e) => GoToPylintMessage(e);

        public void OpenFile(string filePath, bool onBoard = false) {

            if (IsFileOpen(filePath)) {
                MainTabControl.SelectedIndex = GetTabIndexOfOpenFile(filePath);
            } else {
                TextEditor textEditor = AddFile(filePath, onBoard);
                MainTabControl.SelectedIndex = MainTabControl.Items.Count - 1;
                UpdatePylintMessages(textEditor);
            }
        }

        public bool IsFileOpen(string filePath) => GetTabIndexOfOpenFile(filePath) != -1;

        private TextEditor AddFile(string filePath, bool onBoard = false, int atIndex = -1) {
            TextEditor textEditor;

            if (onBoard) {
                textEditor = new BoardTextEditor(filePath, filePath[LocalBoardPath.Length..]);
                ((BoardTextEditor) textEditor).StartedWritingToBoard += (s, e) => { };
                ((BoardTextEditor) textEditor).DoneWritingToBoard += (s, e) => { };
            } else
                textEditor = new(filePath);

            textEditor.OnFileSaved += TextEditor_OnFileSaved;

            if (atIndex >= 0) {
                MainTabControl.Items.Insert(atIndex, new FileTabItem() {
                    Header = Path.GetFileName(filePath).Replace("_", "__"),
                    Content = textEditor,
                });
            } else {
                MainTabControl.Items.Add(new FileTabItem() {
                    Header = Path.GetFileName(filePath).Replace("_", "__"),
                    Content = textEditor,
                });
            }

            if (textEditor.IsPythonFile)
                PythonOnlyFilePaths.Add(filePath);

            OpenTextEditors.Add(textEditor);

            return textEditor;
        }

        private async void UpdatePylintMessages(TextEditor textEditor) {
            if (textEditor.EnablePylinging && GlobalSettings.Default.PylintIsUsable) {
                PylintMessage[] pylintMessages = await MessagesWindow.UpdateLintMessages(PythonOnlyFilePaths.ToArray());
                textEditor.Underliner.Underline(pylintMessages.Where(x => Path.GetFullPath(x.Path) == Path.GetFullPath(textEditor.FilePath)).ToArray(), textEditor.FirstVisibleLineNum, textEditor.LastVisibleLineNum);
            }
        }

        private void TextEditor_OnFileSaved(object? sender, string filePath) => UpdatePylintMessages((TextEditor) sender);

        public void AddTempFile() {
            string newFilePath = $"TempFiles/temp_file{Directory.GetFiles("TempFiles").Length + 1}.py";
            File.Create(newFilePath).Close();
            OpenFile(newFilePath);
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (MainTabControl.SelectedItem is null)
                return;

            OpenTextEditor.DisableAllWrapers = true;
            OpenTextEditor = (TextEditor) ((TabItem) MainTabControl.SelectedItem).Content;
            OpenTextEditor.DisableAllWrapers = false;
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == true)
                OpenFile(openFileDialog.FileName);
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
                OpenFile(s.FilePath);
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
            if (IsFileOpen(filePath)) {
                MainTabControl.Items.RemoveAt(GetTabIndexOfOpenFile(filePath));
            }
        }

        public void OpenBoardDirectory(string directory = "") {
            // "" is the default path, do not use "/"!

            BoardExplorer = new(LocalBoardPath, directory);

            BoardExplorer.OnFileClick += (s) => {
                OpenFile(s.FilePath, true);
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

            if (!Tools.EnableBoardInteractions)
                return;

            AmpyWraper.DownloadDirectoryFromBoard(GlobalSettings.Default.SelectedCOMPort, BoardExplorer.DirectoryPathOnBoard, BoardExplorer.DirectoryPath);

            for (int i = 0; i < OpenTextEditors.Count; i++)
                OpenTextEditors[i].ReloadFile();
        }

        private void RunFileOnBoardButton_Click(object sender, RoutedEventArgs e) {
            DisableBoardInteractions();
            OpenTextEditor.SaveFile();

            if (!Tools.EnableBoardInteractions) {
                ErrorMessager.PromptForCOMPort();
                return;
            }

            OutputTabControl.SelectedIndex = 1;

            AmpyWraper.FileRunner.RunFileOnBoardAsync(GlobalSettings.Default.SelectedCOMPort, OpenTextEditor.FilePath);
        }

        private void RunFileLocalButton_Click(object sender, RoutedEventArgs e) {
            RunFileLocalButton.IsEnabled = false;
            OpenTextEditor.SaveFile();
            OutputTabControl.SelectedIndex = 2;
            PythonWraper.AsyncFileRunner.RunFileAsync(OpenTextEditor.FilePath);
        }

        private void StopAllRunningTasksButton_Click(object sender, RoutedEventArgs e) {
            PythonWraper.AsyncFileRunner.KillProcess();

            if (Tools.EnableBoardInteractions) {
                AmpyWraper.FileRunner.KillProcess();
                AmpyWraper.Softreset(GlobalSettings.Default.SelectedCOMPort);
                EnableBoardInteractions();
            } else
                DisableBoardInteractions();

            RunFileLocalButton.IsEnabled = GlobalSettings.Default.PythonIsInstalled;
        }

        private void GoToPylintMessage(PylintMessage pylintMessage) => GoTo(pylintMessage.Path, pylintMessage.Line, pylintMessage.Column);

        private void GoTo(string filePath, int row, int column) {
            OpenFile(filePath);
            OpenTextEditor.SetCaretPositioin(row, column);
            OpenTextEditor.ScrollToPosition(row, column);
        }

        private int GetTabIndexOfOpenFile(string filePath) {
            for (int i = 0; i < MainTabControl.Items.Count; i++) {
                TabItem tabItem = (TabItem) MainTabControl.Items[i];
                TextEditor content = (TextEditor) tabItem.Content;
                if (Path.GetFullPath(content.FilePath) == Path.GetFullPath(filePath)) {
                    return i;
                }
            }
            return -1;
        }

        private void DisableBoardInteractions() {
            SyncButton.IsEnabled = false;
            RunFileOnBoardButton.IsEnabled = false;

        }

        private void EnableBoardInteractions() {
            SyncButton.IsEnabled = true;
            RunFileOnBoardButton.IsEnabled = true;

        }
    }
}
