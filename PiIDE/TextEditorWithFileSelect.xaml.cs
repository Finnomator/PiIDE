using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace PiIDE {

    public partial class TextEditorWithFileSelect : UserControl {

        private TabItem OpenTabItem;
        private Dictionary<string, int> OpenFilesAndTheirTabindex = new();

        public TextEditorWithFileSelect() {
            InitializeComponent();
            OpenTabItem = DefaultEditor;

            MainTabControl.Items.Clear();

            RootFileView.OnFileClick += RootFileView_OnFileClick;

            OpenFile("TempFiles/temp_file1.py");

            OpenDirectory(@"E:\Users\finnd\Documents\Visual_Studio_Code\MicroPython");
        }

        private void RootFileView_OnFileClick(object? sender, string e) => OpenFile(e);

        public void OpenFile(string filePath) {
            if (IsFileOpen(filePath))
                MainTabControl.SelectedIndex = OpenFilesAndTheirTabindex[filePath];
            else {
                AddFile(filePath);
                OpenFilesAndTheirTabindex[filePath] = MainTabControl.Items.Count - 1;
                MainTabControl.SelectedIndex = MainTabControl.Items.Count - 1;
            }

            MessagesWindow.UpdateLintMessages(OpenFilesAndTheirTabindex.Keys.ToArray());
        }

        public bool IsFileOpen(string filePath) => OpenFilesAndTheirTabindex.ContainsKey(filePath);

        private void AddFile(string filePath) {
            TextEditor textEditor = new(filePath);
            textEditor.OnFileSaved += TextEditor_OnFileSaved;
            MainTabControl.Items.Add(new TabItem() {
                Header = Path.GetFileName(filePath).Replace("_", "__"),
                Content = textEditor,
            });
        }

        private void TextEditor_OnFileSaved(object? sender, string filePath) => MessagesWindow.UpdateLintMessages(OpenFilesAndTheirTabindex.Keys.ToArray());

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
    }
}
