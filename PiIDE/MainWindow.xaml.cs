using System.Collections.Generic;
using System.Windows;

namespace PiIDE {

    public partial class MainWindow : Window {

        // private const string FilePath = @"C:\Users\finnd\source\repos\PiIDE\PiIDE\test_file.py";
        // private const string FilePath = @"C:\Users\finnd\Documents\Visual_Studio_Code\Micropython\Robi42\test.py";
        private const string FilePath = @"E:\Users\finnd\Documents\Visual_Studio_Code\MicroPython\test.py";
        private readonly TextEditor Editor;
        private readonly List<string> FilesToLint = new() { FilePath, @"E:\Users\finnd\Documents\Visual_Studio_Code\MicroPython\test2.py" };

        public MainWindow() {
            InitializeComponent();

            Editor = new(FilePath);
            Editor.OnFileSaved += Editor_OnFileSaved;
            MainGrid.Children.Add(Editor);

            MessagesWindow.UpdateLintMessages(FilesToLint.ToArray());
        }

        private void Editor_OnFileSaved(object? sender, System.EventArgs e) {
            TextEditor textEditor = (TextEditor) sender;
            MessagesWindow.UpdateLintMessages(FilesToLint.ToArray());
        }
    }
}
