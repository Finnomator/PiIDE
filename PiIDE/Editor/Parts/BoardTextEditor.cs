﻿using PiIDE.Wrapers;
using System.Windows;

namespace PiIDE.Editor.Parts {
    public class BoardTextEditor : TextEditor {

        private string BoardFilePath;

        public BoardTextEditor(string filePath, string boardFilePath) : base(filePath) {
            BoardFilePath = boardFilePath;
        }

        public override void SaveFile() {
            base.SaveFile();

            if (!Tools.EnableBoardInteractions) {
                MessageBox.Show("Unable to save file on board", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AmpyWraper.WriteToBoard(GlobalSettings.Default.SelectedCOMPort, FilePath, BoardFilePath);
        }
    }
}
