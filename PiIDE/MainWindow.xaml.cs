using System.Threading.Tasks;
using System.Windows;

namespace PiIDE {

    public partial class MainWindow : Window {

        private readonly string FilePath = "C:\\Users\\finnd\\source\\repos\\PiIDE\\PiIDE\\test_file.py";

        public MainWindow() {
            InitializeComponent();
            Editor.FilePath = FilePath;
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
