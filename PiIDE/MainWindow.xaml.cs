using PiIDE.Wrapers;
using System.Windows;

namespace PiIDE {

    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
            MissingModulesChecker.CheckForUsableModules();

            //JediWraper.Script script = new("", "C:\\Users\\finnd\\source\\repos\\PiIDE\\PiIDE\\TempFiles\\temp_file1.py");
            //script.Complete(1, 0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            GlobalSettings.Default.Save();
        }
    }
}
