using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PiIDE.Editor.Parts.Panel.Terminals;

public abstract partial class TerminalBase {

    private bool ReceivingErrorData;
    private readonly StringBuilder BufferBuilder = new();
    private object LockObject = new();

    protected TerminalBase() {
        InitializeComponent();
        Task.Run(BufferFlushCycle);
    }

    protected void ErrorDataReceived(object sender, DataReceivedEventArgs e) {
        string? data = e.Data;

        if (data == null)
            return;

        ReceivingErrorData = true;

        Dispatcher.Invoke(() => {
            OutputTextBox.Text += $"{data}\n";
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

        lock (LockObject) {
            BufferBuilder.Append($"{data}\n");
        }
    }

    private void BufferFlushCycle() {
        while (true) {

            while (BufferBuilder.Length == 0)
                Thread.Sleep(100);

            lock (LockObject) {
                Dispatcher.Invoke(() => {
                    OutputTextBox.Text += BufferBuilder.ToString();
                    OutputTextBox.ScrollToEnd();
                });
            }

            BufferBuilder.Clear();
            Thread.Sleep(100);
        }
    }

    protected void PrintEndOfExecution(string message) => Dispatcher.Invoke(() => {
        OutputTextBox.Text += $"--------------{message}--------------\n";
        OutputTextBox.ScrollToEnd();
    });

    protected abstract void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e);

    private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e) {
        OutputTextBox.Text = "";
    }
}