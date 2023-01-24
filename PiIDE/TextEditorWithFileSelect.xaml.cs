using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PiIDE {

    public partial class TextEditorWithFileSelect : UserControl {

        private TabItem OpenTabItem;

        private readonly Dictionary<string, int> OpenLocalFilesAndTheirTabindex = new();
        private readonly List<string> PythonOnlyFilePaths = new();
        private readonly List<TextEditor> OpenTextEditors = new();

        public const string LocalBoardPath = "BoardFiles/";

        public TextEditorWithFileSelect() {
            InitializeComponent();
            OpenTabItem = DefaultEditor;

            MainTabControl.Items.Clear();

            RootFileView.OnFileClick += RootFileView_OnFileClick;
            RootBoardFileView.OnFileClick += RootBoardFileView_OnFileClick;

            OpenFile("TempFiles/temp_file1.py");

            OpenDirectory(@"E:\Users\finnd\Documents\Visual_Studio_Code\MicroPython");
            OpenBoardDirectory();
        }

        private void RootBoardFileView_OnFileClick(object? sender, string e) {
            BoardFileViewItem boardFileViewItem = (BoardFileViewItem) sender;
            if (!boardFileViewItem.IsDir)
                OpenFile(e, boardFileViewItem);
        }

        private void RootFileView_OnFileClick(object? sender, string e) => OpenFile(e);

        public void OpenFile(string filePath, BoardFileViewItem? boardItem = null) {

            TextEditor newTextEditor;

            if (IsFileOpen(filePath)) {
                MainTabControl.SelectedIndex = OpenLocalFilesAndTheirTabindex[filePath];
                newTextEditor = OpenTextEditors[MainTabControl.SelectedIndex];
            } else {
                newTextEditor = AddFile(filePath, boardItem);
                MainTabControl.SelectedIndex = MainTabControl.Items.Count - 1;
            }

            UpdatePylintMessages(newTextEditor);
        }

        public bool IsFileOpen(string filePath) => OpenLocalFilesAndTheirTabindex.ContainsKey(filePath);

        private TextEditor AddFile(string filePath, BoardFileViewItem? boardItem = null) {
            TextEditor textEditor = new(filePath, boardItem);
            textEditor.OnFileSaved += TextEditor_OnFileSaved;
            MainTabControl.Items.Add(new TabItem() {
                Header = Path.GetFileName(filePath).Replace("_", "__"),
                Content = textEditor,
            });

            if (textEditor.IsPythonFile)
                PythonOnlyFilePaths.Add(filePath);

            OpenLocalFilesAndTheirTabindex[filePath] = MainTabControl.Items.Count - 1;
            OpenTextEditors.Add(textEditor);

            return textEditor;
        }

        private async void UpdatePylintMessages(TextEditor textEditor) {
            if (textEditor.EnablePylinging) {
                PylintMessage[] pylintMessages = await MessagesWindow.UpdateLintMessages(PythonOnlyFilePaths.ToArray());
                textEditor.Underliner.Underline(pylintMessages, textEditor.FirstVisibleLineNum, textEditor.LastVisibleLineNum);
            }
        }

        private void TextEditor_OnFileSaved(object? sender, string filePath) => UpdatePylintMessages((TextEditor) sender);

        public void AddTempFile() {
            string newFilePath = $"TempFiles/temp_file{Directory.GetFiles("TempFiles").Length + 1}.py";
            File.Create(newFilePath).Close();
            AddFile(newFilePath);
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            TextEditor oldTextEditor = (TextEditor) OpenTabItem.Content;
            oldTextEditor.DisableAllWrapers = true;

            OpenTabItem = (TabItem) MainTabControl.SelectedItem;
            TextEditor newTextEditor = (TextEditor) OpenTabItem.Content;
            newTextEditor.DisableAllWrapers = false;
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

        public void OpenBoardDirectory(string directory = "") => RootBoardFileView.OpenDir(directory, LocalBoardPath, 9, 0);

        private void SyncButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            Debug.Assert(RootBoardFileView.IsRootDir);
            RootBoardFileView.DownloadDirectory(LocalBoardPath);
            for (int i = 0; i < OpenTextEditors.Count; i++) {
                OpenTextEditors[i].ReloadFile();
            }
        }

        private void RunFileOnBoardButton_Click(object sender, System.Windows.RoutedEventArgs e) {
            TextEditor openEditor = (TextEditor) OpenTabItem.Content;
            openEditor.SaveFile();
            AmpyWraper.RunFileOnBoardAsync(9, openEditor.FilePath);
        }
    }
}
