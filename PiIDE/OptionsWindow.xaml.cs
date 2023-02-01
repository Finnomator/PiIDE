using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace PiIDE {

    public partial class OptionsWindow : Window {
        public OptionsWindow() {
            InitializeComponent();

            int port = GlobalSettings.Default.SelectedCOMPort;
            if (port >= 0) {
                COMPortComboBox.Items.Add(new TextBlock() {
                    Text = $"COM{port}",
                    Tag = port
                });
                COMPortComboBox.SelectedIndex = 1;
            }

            try {
                VersionLabel.Content = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            } catch { }
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

            int selectedCOMPort;

            if (COMPortComboBox.SelectedIndex <= 0)
                selectedCOMPort = -1;
            else
                selectedCOMPort = (int) ((TextBlock) COMPortComboBox.SelectedItem).Tag;

            GlobalSettings.Default.SelectedCOMPort = selectedCOMPort;
            GlobalSettings.Default.Save();
        }

        private void GithubIssue_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void GithubRepo_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
