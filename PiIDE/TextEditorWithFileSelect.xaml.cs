using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace PiIDE {

    public partial class TextEditorWithFileSelect : UserControl {

        private TextEditor OpenTextEditor;

        private readonly Dictionary<string, int> OpenLocalFilesAndTheirTabindex = new();
        private readonly List<string> PythonOnlyFilePaths = new();
        private readonly List<TextEditor> OpenTextEditors = new();

        public const string LocalBoardPath = "BoardFiles/";

        public TextEditorWithFileSelect() {
            InitializeComponent();
            OpenTextEditor = (TextEditor) DefaultEditor.Content;

            MainTabControl.Items.Clear();

            RootFileView.OnFileClick += RootFileView_OnFileClick;
            RootBoardFileView.OnFileClick += RootBoardFileView_OnFileClick;
            MessagesWindow.SelectionChanged += MessagesWindow_SelectionChanged;

            AmpyWraper.AmpyExited += Ampy_Exited;
            PythonWraper.PythonExited += Python_Exited;

            OpenFile("TempFiles/temp_file1.py");
            // TODO: Make these paths variable
            //OpenDirectory(@"E:\Users\finnd\Documents\Visual_Studio_Code\MicroPython");
            OpenDirectory("C:\\Users\\finnd\\Documents\\Visual_Studio_Code");
            // TODO: Reopen board directory when comport gets changed
            if (GlobalSettings.Default.SelectedCOMPort >= 0)
                OpenBoardDirectory();
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

        private void RootBoardFileView_OnFileClick(object? sender, string e) {
            BoardFileViewItem boardFileViewItem = (BoardFileViewItem) sender;
            if (!boardFileViewItem.IsDir)
                OpenFile(e, boardFileViewItem);
        }

        private void RootFileView_OnFileClick(object? sender, string e) => OpenFile(e);

        public void OpenFile(string filePath, BoardFileViewItem? boardItem = null) {

            if (IsFileOpen(filePath)) {
                MainTabControl.SelectedIndex = OpenLocalFilesAndTheirTabindex[Path.GetFullPath(filePath)];
            } else {
                TextEditor newTextEditor = AddFile(filePath, boardItem);
                MainTabControl.SelectedIndex = MainTabControl.Items.Count - 1;
                UpdatePylintMessages(newTextEditor);
            }
        }

        public bool IsFileOpen(string filePath) => OpenLocalFilesAndTheirTabindex.Keys.Any(x => Path.GetFullPath(filePath) == Path.GetFullPath(x));

        private TextEditor AddFile(string filePath, BoardFileViewItem? boardItem = null) {
            TextEditor textEditor = new(filePath, boardItem);
            textEditor.OnFileSaved += TextEditor_OnFileSaved;
            MainTabControl.Items.Add(new TabItem() {
                Header = Path.GetFileName(filePath).Replace("_", "__"),
                Content = textEditor,
            });

            if (textEditor.IsPythonFile)
                PythonOnlyFilePaths.Add(filePath);

            OpenLocalFilesAndTheirTabindex[Path.GetFullPath(filePath)] = MainTabControl.Items.Count - 1;
            OpenTextEditors.Add(textEditor);

            return textEditor;
        }

        private async void UpdatePylintMessages(TextEditor textEditor) {
            if (textEditor.EnablePylinging) {
                PylintMessage[] pylintMessages = await MessagesWindow.UpdateLintMessages(PythonOnlyFilePaths.ToArray());
                textEditor.Underliner.Underline(pylintMessages.Where(x => Path.GetFullPath(x.Path) == Path.GetFullPath(textEditor.FilePath)).ToArray(), textEditor.FirstVisibleLineNum, textEditor.LastVisibleLineNum);
            }
        }

        private void TextEditor_OnFileSaved(object? sender, string filePath) => UpdatePylintMessages((TextEditor) sender);

        public void AddTempFile() {
            string newFilePath = $"TempFiles/temp_file{Directory.GetFiles("TempFiles").Length + 1}.py";
            File.Create(newFilePath).Close();
            AddFile(newFilePath);
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            OpenTextEditor.DisableAllWrapers = true;
            OpenTextEditor = (TextEditor) ((TabItem) MainTabControl.SelectedItem).Content;
            OpenTextEditor.DisableAllWrapers = false;
        }

        private void OpenFileButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new();
            if (openFileDialog.ShowDialog() == true)
                AddFile(openFileDialog.FileName);
        }

        private void CreateNewFileButton_Click(object sender, System.Windows.RoutedEventArgs e) => AddTempFile();

        public void OpenDirectory(string directory) {
            RootPathTextBox.Text = directory;
            RootFileView.OpenDir(true, directory, 0);
        }

        public void OpenBoardDirectory(string directory = "") {
            // "" is the default path, do not use "/"!
            if (GlobalSettings.Default.SelectedCOMPort < 0) {
                ErrorMessager.PromptForCOMPort();
                return;
            }

            RootBoardFileView.OpenDir(directory, LocalBoardPath, GlobalSettings.Default.SelectedCOMPort, 0);
        }

        private void SyncButton_Click(object sender, System.Windows.RoutedEventArgs e) {

            if (GlobalSettings.Default.SelectedCOMPort < 0) {
                ErrorMessager.PromptForCOMPort();
                return;
            }

            Debug.Assert(RootBoardFileView.IsRootDir);
            RootBoardFileView.DownloadDirectory(LocalBoardPath);
            for (int i = 0; i < OpenTextEditors.Count; i++) {
                OpenTextEditors[i].ReloadFile();
            }
        }

        private void RunFileOnBoardButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            RunFileOnBoardButton.IsEnabled = false;
            OpenTextEditor.SaveFile();

            int port = GlobalSettings.Default.SelectedCOMPort;

            if (!Tools.IsValidCOMPort(port)) {
                ErrorMessager.PromptForCOMPort();
                return;
            }

            OutputTabControl.SelectedIndex = 1;

            AmpyWraper.FileRunner.RunFileOnBoardAsync(port, OpenTextEditor.FilePath);
        }

        private void RunFileLocalButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            RunFileLocalButton.IsEnabled = false;
            OpenTextEditor.SaveFile();
            OutputTabControl.SelectedIndex = 2;
            PythonWraper.AsyncFileRunner.RunFileAsync(OpenTextEditor.FilePath);
        }

        private void StopAllRunningTasksButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            PythonWraper.AsyncFileRunner.KillProcess();
            AmpyWraper.FileRunner.KillProcess();
            RunFileLocalButton.IsEnabled = true;
            RunFileOnBoardButton.IsEnabled = true;
        }

        private void GoToPylintMessage(PylintMessage pylintMessage) {
            OpenFile(pylintMessage.Path);
            OpenTextEditor.SetCaretPositioin(pylintMessage.Line, pylintMessage.Column);
            OpenTextEditor.ScrollToPosition(pylintMessage.Line, pylintMessage.Column);
        }
    }
}
