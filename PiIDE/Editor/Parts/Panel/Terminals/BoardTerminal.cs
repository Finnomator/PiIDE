﻿using System.Windows.Input;
using PiIDE.Wrapers;

namespace PiIDE.Editor.Parts.Panel.Terminals;

// TODO: Ampy throws an exception when waiting for input

public class BoardTerminal : TerminalBase {

    public BoardTerminal() {
        AmpyWraper.AmpyOutputDataReceived += OutputDataReceived;
        AmpyWraper.AmpyErrorDataReceived += ErrorDataReceveid;
        AmpyWraper.AmpyExited += Exited;
    }

    protected override void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
        switch (e.Key) {
            case Key.Enter:
                AmpyWraper.FileRunner.WriteLineToRunningFileInput(InputTextBox.Text);
                InputTextBox.Text = "";
                e.Handled = true;
                break;
        }
    }
}