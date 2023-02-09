using PiIDE.Wrapers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Editor.Parts {
    public class BoardTextEditor : TextEditor {

        public event EventHandler? StartedWritingToBoard;
        public event EventHandler? DoneWritingToBoard;
        public event EventHandler? StartedPythonExecutionOnBoard;

        private readonly string BoardFilePath;
        private readonly Button RunFileOnBoardButton = new() {
            ToolTip = "Upload to Board and run",
            Foreground = Brushes.LightGreen,
            Background = Brushes.Transparent,
            BorderThickness = new(0),
            Padding = new(3),
            Content = new FontAwesome.WPF.FontAwesome() {
                Icon = FontAwesome.WPF.FontAwesomeIcon.Play,
            }
        };

        public BoardTextEditor(string filePath, string boardFilePath, bool disableAllWrapers = false) : base(filePath, disableAllWrapers) {
            BoardFilePath = boardFilePath;
            RunFileOnBoardButton.Click += RunFileOnBoardButton_Click;
            AmpyWraper.AmpyExited += Ampy_Exited;
            ActionsStackPanel.Children.Add(RunFileOnBoardButton);
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

        private void DisableBoardInteractions() => RunFileOnBoardButton.IsEnabled = false;

        private void EnableBoardInteractions() => RunFileOnBoardButton.IsEnabled = true;

        protected override void StopAllRunningTasksButton_Click(object sender, RoutedEventArgs e) {
            base.StopAllRunningTasksButton_Click(sender, e);
            if (Tools.EnableBoardInteractions) {
                AmpyWraper.FileRunner.KillProcess();
                AmpyWraper.Softreset(GlobalSettings.Default.SelectedCOMPort);
                EnableBoardInteractions();
            } else
                DisableBoardInteractions();
        }

        private void RunFileOnBoardButton_Click(object sender, RoutedEventArgs e) {

            DisableBoardInteractions();

            if (!Tools.EnableBoardInteractions) {
                ErrorMessager.PromptForCOMPort();
                DisableBoardInteractions();
                return;
            }

            AmpyWraper.FileRunner.RunFileOnBoardAsync(GlobalSettings.Default.SelectedCOMPort, FilePath);
            StartedPythonExecutionOnBoard?.Invoke(this, EventArgs.Empty);
        }

        private void Ampy_Exited(object? sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                RunFileOnBoardButton.IsEnabled = true;
            });
        }
    }
}
