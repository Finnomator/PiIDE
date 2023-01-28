using System.Windows;

namespace PiIDE {

    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            GlobalSettings.Default.Save();
        }
    }
}
