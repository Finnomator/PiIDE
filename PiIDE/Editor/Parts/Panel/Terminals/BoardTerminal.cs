using PiIDE.Wrappers;
using System.Windows.Input;

namespace PiIDE.Editor.Parts.Panel.Terminals;

// TODO: Ampy throws an exception when waiting for input

public class BoardTerminal : TerminalBase {

    public BoardTerminal() {
        AmpyWrapper.AmpyOutputDataReceived += OutputDataReceived;
        AmpyWrapper.AmpyErrorDataReceived += ErrorDataReceived;
        AmpyWrapper.AmpyExited += Exited;
    }

    protected override void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
        switch (e.Key) {
            case Key.Enter:
                AmpyWrapper.FileRunner.WriteLineToRunningFileInput(InputTextBox.Text);
                InputTextBox.Text = "";
                e.Handled = true;
                break;
        }
    }
}