using PiIDE.Options;
using System.Windows;

namespace PiIDE;

public partial class Toolbar {

    public Toolbar() => InitializeComponent();

    private void OptionsButton_Click(object sender, RoutedEventArgs e) {
        OptionsWindow optionsWindow = new();
        optionsWindow.Show();
    }
}