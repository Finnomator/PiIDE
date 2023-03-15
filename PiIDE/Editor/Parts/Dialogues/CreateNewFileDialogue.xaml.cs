using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;


namespace PiIDE.Editor.Parts.Dialogues {
    public partial class CreateNewFileDialogue : Window {

        // TODO: make if file gets closed by not the cancel button that CreateNewFileDialogueResult gets set to None

        public CreateNewFileDialogueResult CreateNewFileDialogueResult { get; private set; }
        public string FileName => FileNameTextBox.Text;
        public string SourceFolder => (string) SourceFolderButton.Content;
        public string FilePath => Path.Combine(SourceFolder, FileName);

        private bool CreateClose;

        public CreateNewFileDialogue() {
            InitializeComponent();

            Loaded += delegate {
                CreateNewFileDialogueResult = CreateNewFileDialogueResult.Local;
            };

            PiRadioButton.IsEnabled = Tools.EnableBoardInteractions;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            CreateNewFileDialogueResult = CreateNewFileDialogueResult.None;
            Close();
        }

        private void Create_Click(object sender, RoutedEventArgs e) {

            bool isValid = !string.IsNullOrEmpty(FileName) &&
              FileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
              !File.Exists(FilePath);

            if (isValid) {
                CreateClose = true;
                Close();
                return;
            }

            // TODO: Make this prettier
            MessageBox.Show("The filename is invalid or the file already exists");
        }

        private void Disk_Checked(object sender, RoutedEventArgs e) {

            if (SourceFolderButton is null)
                return;

            CreateNewFileDialogueResult = CreateNewFileDialogueResult.Local;
            SourceFolderButton.IsEnabled = true;
            SourceFolderButton.Content = "C:\\";
        }

        private void PiRadioButton_Checked(object sender, RoutedEventArgs e) {
            CreateNewFileDialogueResult = CreateNewFileDialogueResult.Pi;
            SourceFolderButton.IsEnabled = false;
            SourceFolderButton.Content = "Pi/";
        }

        private void SourceFolderButton_Click(object sender, RoutedEventArgs e) {

            if (CreateNewFileDialogueResult != CreateNewFileDialogueResult.Local)
                return;

            using FolderBrowserDialog fbd = new();
            DialogResult result = fbd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                SourceFolderButton.Content = fbd.SelectedPath;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (CreateClose) {
            } else {
                CreateNewFileDialogueResult = CreateNewFileDialogueResult.None;
            }
        }
    }

    public enum CreateNewFileDialogueResult {
        None,
        Local,
        Pi,
    }
}
