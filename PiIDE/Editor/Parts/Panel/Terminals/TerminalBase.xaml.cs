using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace PiIDE {
    public abstract partial class TerminalBase : UserControl {

        public TerminalBase() {
            InitializeComponent();
        }

        protected void Exited(object? sender, EventArgs e) {

            // Happens when the cmd returns but not when all output is done

            /*
            Dispatcher.Invoke(() => {
                OutputTextBox.Text += "-----------------------------\r\n";
                OutputTextBox.ScrollToEnd();
            });
            */
        }

        protected void ErrorDataReceveid(object sender, DataReceivedEventArgs e) {
            string? data = e.Data;

            if (data == null) {
                return;
            }

            data += "\r\n";

            Dispatcher.Invoke(() => {
                OutputTextBox.Text += data;
                OutputTextBox.ScrollToEnd();
            });
        }

        protected void OutputDataReceived(object sender, DataReceivedEventArgs e) {
            // this will only be called when a newline is printed

            string? data = e.Data;

            if (data == null) {
                PrintEndOfExecution();
                return;
            }

            data += "\r\n";

            Dispatcher.Invoke(() => {
                OutputTextBox.Text += data;
                OutputTextBox.ScrollToEnd();
            });
        }

        protected void PrintEndOfExecution() {
            Dispatcher.Invoke(() => {
                OutputTextBox.Text += "-----------------------------\r\n";
                OutputTextBox.ScrollToEnd();
            });
        }

        protected abstract void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e);
    }
}
