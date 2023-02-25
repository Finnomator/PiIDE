using System.Windows;
using System.Windows.Controls;

namespace PiIDE.Options.General {

    public partial class General : UserControl {
        public General() {
            InitializeComponent();

            GlobalSettings settings = GlobalSettings.Default;

            int port = settings.SelectedCOMPort;
            if (port >= 0) {
                COMPortComboBox.Items.Add(new TextBlock() {
                    Text = $"COM{port}",
                    Tag = port
                });
                COMPortComboBox.SelectedIndex = 1;
            } else
                COMPortComboBox.SelectedIndex = 0;
        }

        private void COMPortComboBox_DropDownOpened(object sender, System.EventArgs e) {
            COMPortComboBox.Items.Clear();
            COMPortComboBox.Items.Add("<None>");
            foreach (int port in Tools.GetCOMPorts())
                COMPortComboBox.Items.Add(new TextBlock() {
                    Text = $"COM{port}",
                    Tag = port
                });
            COMPortComboBox.SelectedIndex = 0;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            int selectedCOMPort = COMPortComboBox.SelectedIndex <= 0 ? -1 : (int) ((TextBlock) COMPortComboBox.SelectedItem).Tag;
            GlobalSettings.Default.SelectedCOMPort = selectedCOMPort;
        }
    }
}
