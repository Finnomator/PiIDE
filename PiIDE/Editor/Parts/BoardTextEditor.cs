using PiIDE.Wrapers;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace PiIDE.Editor.Parts {
    public class BoardTextEditor : TextEditor {

        public event EventHandler? StartedWritingToBoard;
        public event EventHandler? DoneWritingToBoard;

        private readonly string BoardFilePath;

        public BoardTextEditor(string filePath, string boardFilePath) : base(filePath) {
            BoardFilePath = boardFilePath;
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
        }
    }
}
