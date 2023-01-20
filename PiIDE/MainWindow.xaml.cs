using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace PiIDE {

    public partial class MainWindow : Window {

        // private const string FilePath = @"C:\Users\finnd\source\repos\PiIDE\PiIDE\test_file.py";
        private const string FilePath = @"C:\Users\finnd\Documents\Visual_Studio_Code\Micropython\Robi42\test.py";
        private readonly TextEditor Editor;

        public MainWindow() {
            InitializeComponent();

            Editor = new(FilePath);
            MainGrid.Children.Add(Editor);

            Process process = new Process() {
                StartInfo = new ProcessStartInfo() {
                    FileName = "Assets/Pygmentize/pygmentize.exe",
                    Arguments = FilePath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow= true
                }
            
            };

            process.Start();
            var e = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            UpdateLintMessages();
        }

        private async void UpdateLintMessages() {
            while (true) {
                PylintMessage[] pylintMessages = await PylintWraper.GetLintingAsync(FilePath);

                MessagesWindow.ClearLintMessages();
                MessagesWindow.AddLintMessages(pylintMessages);

                await Task.Delay(1000);
            }
        }
    }
}
