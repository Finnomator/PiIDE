using System;
using System.Windows;
using PiIDE.Options;

namespace PiIDE;

public partial class Toolbar {

    public EventHandler? OnOpenOptionsCLick;

    public Toolbar() => InitializeComponent();

    private void OptionsButton_Click(object sender, RoutedEventArgs e) {
        OnOpenOptionsCLick?.Invoke(sender, e);
        OptionsWindow optionsWindow = new();
        optionsWindow.Show();
    }
}