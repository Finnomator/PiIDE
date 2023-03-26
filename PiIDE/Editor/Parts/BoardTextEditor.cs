﻿using PiIDE.Wrapers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Editor.Parts {
    public class BoardTextEditor : TextEditor {

        public event EventHandler? StartedPythonExecutionOnBoard;

        private readonly string BoardFilePath;

        // Additional UI Elements
        private readonly Button RunFileOnBoardButton = new() {
            ToolTip = "Upload to Board and run",
            Foreground = Brushes.LightGreen,
            Background = Brushes.Transparent,
            BorderThickness = new(0),
            Padding = new(3),
            Style = (Style) Application.Current.Resources["CleanButtonStyle"],
            Content = new FontAwesome.WPF.FontAwesome() {
                Icon = FontAwesome.WPF.FontAwesomeIcon.Play,
            }
        };
        private readonly WrapPanel UploadingFileWrapPanel = new();

        public BoardTextEditor(string filePath, string boardFilePath, bool disableAllWrapers = false) : base(filePath, disableAllWrapers) {
            BoardFilePath = boardFilePath;
            RunFileOnBoardButton.Click += RunFileOnBoardButton_Click;
            AmpyWraper.AmpyExited += Ampy_Exited;

            ActionsStackPanel.Children.Add(RunFileOnBoardButton);


            UploadingFileWrapPanel.Visibility = Visibility.Collapsed;
            UploadingFileWrapPanel.Children.Add(new TextBlock() {
                Text = "Uploading File",
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new(20, 0, 2, 0),
            });
            UploadingFileWrapPanel.Children.Add(Tools.NewWpfSpinner());

            InformationWrapPanel.Children.Add(UploadingFileWrapPanel);
        }

        public override async Task SaveFileAsync(bool savedByUser) {
            await base.SaveFileAsync(savedByUser);

            if (!savedByUser)
                return;

            if (!Tools.EnableBoardInteractions) {
                MessageBox.Show("Unable to save file on board", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SaveFileButton.IsEnabled = true;
                return;
            }

            UploadingFileWrapPanel.Visibility = Visibility.Visible;
            await AmpyWraper.WriteToBoardAsync(GlobalSettings.Default.SelectedCOMPort, FilePath, BoardFilePath);
            UploadingFileWrapPanel.Visibility = Visibility.Collapsed;
        }

        private void DisableBoardInteractions() => RunFileOnBoardButton.IsEnabled = false;

        private void EnableBoardInteractions() => RunFileOnBoardButton.IsEnabled = true;

        protected async override void StopAllRunningTasksButton_Click(object sender, RoutedEventArgs e) {
            base.StopAllRunningTasksButton_Click(sender, e);
            if (Tools.EnableBoardInteractions) {
                AmpyWraper.FileRunner.KillProcess();
                await AmpyWraper.Softreset(GlobalSettings.Default.SelectedCOMPort);
                EnableBoardInteractions();
            } else
                DisableBoardInteractions();
        }

        private async void RunFileOnBoardButton_Click(object sender, RoutedEventArgs e) {

            DisableBoardInteractions();

            if (!Tools.EnableBoardInteractions) {
                ErrorMessager.PromptForCOMPort();
                return;
            }

            await SaveFileAsync(true);
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
