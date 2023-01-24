using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace PiIDE {
    public partial class IntegratedTerminal : UserControl {

        public IntegratedTerminal() {
            InitializeComponent();
            AmpyWraper.AmpyOutputDataReceived += Ampy_OutputDataReceived;
            AmpyWraper.AmpyErrorDataReceived += Ampy_ErrorDataReceveid;
            AmpyWraper.AmpyExited += Ampy_Exited;
        }

        private void Ampy_Exited(object? sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                OutputTextBox.Text += "\r\n--------------------\r\n";
            });
        }

        private void Ampy_ErrorDataReceveid(object sender, DataReceivedEventArgs e) {
            string? data = e.Data;

            if (data == null)
                return;

            Dispatcher.Invoke(() => {
                OutputTextBox.Text += data;
            });
        }

        private void Ampy_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            string? data = e.Data;

            if (data == null)
                return;

            Dispatcher.Invoke(() => {
                OutputTextBox.Text += data;
            });
        }

        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {

        }
    }
}
