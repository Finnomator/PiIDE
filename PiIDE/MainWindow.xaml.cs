using System.Windows;

namespace PiIDE {

    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            MainGrid.Children.Add(new TextEditor("C:\\Users\\finnd\\Documents\\Visual_Studio_Code\\Micropython\\Robi42\\test.py"));
        }
    }
}
