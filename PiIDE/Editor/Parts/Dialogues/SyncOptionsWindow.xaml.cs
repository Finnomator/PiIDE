using System.Windows;

namespace PiIDE.Editor.Parts {

    public partial class SyncOptionsWindow : Window {

        public SyncOptionResult SyncOptionResult { get; private set; }

        private bool OkClose;

        public SyncOptionsWindow() {
            InitializeComponent();

            Loaded += delegate {
                SyncOptionResult = SyncOptionResult.OverwriteAllLocalFiles;
            };
        }

        private void OverwriteAllLocalFile_Checked(object sender, RoutedEventArgs e) {
            SyncOptionResult = SyncOptionResult.OverwriteAllLocalFiles;
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            OkClose = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            SyncOptionResult = SyncOptionResult.Cancel;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (OkClose) {

            } else {
                SyncOptionResult = SyncOptionResult.Cancel;
            }
        }
    }

    public enum SyncOptionResult {
        OverwriteAllLocalFiles,
        Cancel,
    }
}
