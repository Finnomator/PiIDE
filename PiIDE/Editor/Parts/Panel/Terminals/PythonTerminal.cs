using System.Windows.Input;
using PiIDE.Wrapers;

namespace PiIDE.Editor.Parts.Panel.Terminals;

public class PythonTerminal : TerminalBase {

    public PythonTerminal() {
        PythonWraper.PythonOutputDataReceived += OutputDataReceived;
        PythonWraper.PythonErrorDataReceived += ErrorDataReceveid;
        PythonWraper.PythonExited += Exited;
    }

    protected override void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
        switch (e.Key) {
            case Key.Enter:
                PythonWraper.AsyncFileRunner.WriteLineToInput(InputTextBox.Text);
                InputTextBox.Text = "";
                e.Handled = true;
                break;
        }
    }
}