using System.Threading.Tasks;
using System.Windows;

namespace PiIDE {

    public partial class MainWindow : Window {

        // private const string FilePath = @"C:\Users\finnd\source\repos\PiIDE\PiIDE\test_file.py";
        // private const string FilePath = @"C:\Users\finnd\Documents\Visual_Studio_Code\Micropython\Robi42\test.py";
        private const string FilePath = @"E:\Users\finnd\Documents\Visual_Studio_Code\MicroPython\test.py";
        private readonly TextEditor Editor;

        public MainWindow() {
            InitializeComponent();

            Editor = new(FilePath);
            MainGrid.Children.Add(Editor);

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
