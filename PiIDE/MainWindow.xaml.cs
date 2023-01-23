using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PiIDE {

    public partial class MainWindow : Window {

        // private const string FilePath = @"C:\Users\finnd\source\repos\PiIDE\PiIDE\test_file.py";
        private const string FilePath = @"C:\Users\finnd\Documents\Visual_Studio_Code\Micropython\Robi42\test.py";
        // private const string FilePath = @"E:\Users\finnd\Documents\Visual_Studio_Code\MicroPython\test.py";
        private readonly List<string> FilesToLint = new() { FilePath };

        public MainWindow() {
            InitializeComponent();
            MainTabbar.AddFile(FilePath);
            MessagesWindow.UpdateLintMessages(new string[] { "TempFiles/temp_file1.py" });
        }

        private void Editor_OnFileSaved(object? sender, System.EventArgs e) {
            TextEditor textEditor = (TextEditor) sender;
            MessagesWindow.UpdateLintMessages(FilesToLint.ToArray());
        }
    }
}
