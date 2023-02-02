using PiIDE.Wrapers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PiIDE.Editor.Parts {
    public class BoardTextEditor : TextEditor {

        public event EventHandler? StartedWritingToBoard;
        public event EventHandler? DoneWritingToBoard;

        public bool ContentIsSavedOnBoard { get; set; } = true;

        private readonly string BoardFilePath;

        public BoardTextEditor(string filePath, string boardFilePath) : base(filePath) {
            BoardFilePath = boardFilePath;
        }

        protected override void TextEditorTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            base.TextEditorTextBox_TextChanged(sender, e);
            ContentIsSavedOnBoard = false;
        }

        public override async void SaveFile() {
            base.SaveFile();

            if (!Tools.EnableBoardInteractions) {
                MessageBox.Show("Unable to save file on board", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            StartedWritingToBoard?.Invoke(this, EventArgs.Empty);
            await AmpyWraper.WriteToBoardAsync(GlobalSettings.Default.SelectedCOMPort, FilePath, BoardFilePath);
            DoneWritingToBoard?.Invoke(this, EventArgs.Empty);
            ContentIsSavedOnBoard = true;
        }
    }
}
