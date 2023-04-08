using PiIDE.Wrappers;
using System.Windows.Input;

namespace PiIDE.Editor.Parts.Panel.Terminals;

public class PythonTerminal : TerminalBase {

    public PythonTerminal() {
        PythonWrapper.PythonOutputDataReceived += OutputDataReceived;
        PythonWrapper.PythonErrorDataReceived += ErrorDataReceived;
        PythonWrapper.PythonExited += Exited;
    }

    protected override void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
        switch (e.Key) {
            case Key.Enter:
                PythonWrapper.AsyncFileRunner.WriteLineToInput(InputTextBox.Text);
                InputTextBox.Text = "";
                e.Handled = true;
                break;
        }
    }
}