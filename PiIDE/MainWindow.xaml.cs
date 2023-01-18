using System.Windows;

namespace PiIDE {

    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            MainGrid.Children.Add(new TextEditor("C:\\Users\\finnd\\source\\repos\\PiIDE\\PiIDE\\test_file.py"));
        }
    }
}
