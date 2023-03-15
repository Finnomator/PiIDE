using System.Windows;

namespace PiIDE.Editor.Parts {

    public partial class SyncOptionsWindow : Window {

        public SyncOptionResult SyncOptionResult { get; private set; }

        public SyncOptionsWindow() {
            InitializeComponent();

            Loaded += delegate {
                SyncOptionResult = SyncOptionResult.Cancel;
            };
        }

        private void OverwriteAllLocalFile_Checked(object sender, RoutedEventArgs e) {
            SyncOptionResult = SyncOptionResult.OverwriteAllLocalFiles;
        }

        private void Ok_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            SyncOptionResult = SyncOptionResult.Cancel;
            Close();
        }
    }

    public enum SyncOptionResult {
        OverwriteAllLocalFiles,
        Cancel,
    }
}
