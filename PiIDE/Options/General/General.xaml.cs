using System.Windows;
using System.Windows.Controls;

namespace PiIDE.Options.General;

public partial class General {
    public General() {
        InitializeComponent();

        GlobalSettings settings = GlobalSettings.Default;

        int port = settings.SelectedCOMPort;
        if (port >= 0) {
            ComPortComboBox.Items.Add(new TextBlock() {
                Text = $"COM{port}",
                Tag = port
            });
            ComPortComboBox.SelectedIndex = 1;
        } else
            ComPortComboBox.SelectedIndex = 0;
    }

    private void COMPortComboBox_DropDownOpened(object sender, System.EventArgs e) {
        ComPortComboBox.Items.Clear();
        ComPortComboBox.Items.Add("<None>");
        foreach (int port in Tools.GetComPorts())
            ComPortComboBox.Items.Add(new TextBlock() {
                Text = $"COM{port}",
                Tag = port
            });
        ComPortComboBox.SelectedIndex = 0;
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
        int selectedComPort = ComPortComboBox.SelectedIndex <= 0 ? -1 : (int) ((TextBlock) ComPortComboBox.SelectedItem).Tag;
        GlobalSettings.Default.SelectedCOMPort = selectedComPort;
    }
}