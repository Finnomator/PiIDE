using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;

namespace PiIDE.Editor.Parts.Panel.Terminals;

public abstract partial class TerminalBase {

    private bool ReceivingErrorData;

    protected TerminalBase() => InitializeComponent();

    protected void Exited(object? sender, EventArgs e) {

        // Happens when the cmd returns but not when all output is done

        /*
        Dispatcher.Invoke(() => {
            OutputTextBox.Text += "-----------------------------\r\n";
            OutputTextBox.ScrollToEnd();
        });
        */
    }

    protected void ErrorDataReceived(object sender, DataReceivedEventArgs e) {
        string? data = e.Data;

        if (data == null) {
            return;
        }

        ReceivingErrorData = true;

        data += "\r\n";

        Dispatcher.Invoke(() => {
            OutputTextBox.Text += data;
            OutputTextBox.ScrollToEnd();
        });

        ReceivingErrorData = false;
    }

    protected void OutputDataReceived(object sender, DataReceivedEventArgs e) {
        // this will only be called when a newline is printed

        string? data = e.Data;

        if (data == null) {
            Thread.Sleep(10); // Wait for possible Error Output
            for (int i = 0; i < 10 && ReceivingErrorData; ++i)
                Thread.Sleep(10);

            PrintEndOfExecution("Program Finished");
            return;
        }

        data += "\r\n";

        Dispatcher.Invoke(() => {
            OutputTextBox.Text += data;
            OutputTextBox.ScrollToEnd();
        });
    }

    protected void PrintEndOfExecution(string message) => Dispatcher.Invoke(() => {
        OutputTextBox.Text += $"--------------{message}--------------\r\n";
        OutputTextBox.ScrollToEnd();
    });

    protected abstract void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e);
}