using System.Windows;

namespace PiIDE {

    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
            MissingModulesChecker.CheckForUsableModules();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

            if (!Editor.AreAllFilesSaved()) {
                MessageBoxResult msgbr = MessageBox.Show("Some files are not saved. Exit anyway?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (msgbr == MessageBoxResult.No)
                    e.Cancel = true;
            }

            GlobalSettings.Default.Save();
        }
    }
}
