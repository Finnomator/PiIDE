using PiIDE.Options.Editor.SyntaxHighlighter.Colors;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace PiIDE {

    public partial class OptionsWindow : Window {
        public OptionsWindow() {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings() {

            GlobalSettings settings = GlobalSettings.Default;

            // General Tab
            int port = settings.SelectedCOMPort;
            if (port >= 0) {
                COMPortComboBox.Items.Add(new TextBlock() {
                    Text = $"COM{port}",
                    Tag = port
                });
                COMPortComboBox.SelectedIndex = 1;
            } else
                COMPortComboBox.SelectedIndex = 0;

            // Editor Tab
            FontSizeTextBox.Text = settings.TextEditorFontSize.ToString();
            FontFamilyComboBox.SelectedIndex = 0;
            HighlightingModeComboBox.SelectedIndex = settings.SyntaxhighlighterMode;
            HighlighterPerformanceModeComboBox.SelectedIndex = settings.SyntaxhighlighterPerformanceMode;
            CompletionsComboBox.SelectedIndex = settings.CompletionsMode;

            // About Tab
            VersionLabel.Content = Environment.GetEnvironmentVariable("ClickOnce_CurrentVersion") ?? "Not installed";
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

            Shortcuts.SaveShortcuts(Options.Editor.Shortcuts.Shortcuts.ShortcutsJsonPath, Shortcuts.ShortcutsMap);
            ColorResources.SaveResource(ColorOptions.ColorsJsonPath, ColorResources.HighlighterColors.Colors);
        }

        private void GithubIssue_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void GithubRepo_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }


        private void SyntaxHighlighterPerformance_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            GlobalSettings.Default.SyntaxhighlighterPerformanceMode = ((ComboBox) sender).SelectedIndex;
        }

        private void HighlightingMode_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            GlobalSettings.Default.SyntaxhighlighterMode = ((ComboBox) sender).SelectedIndex;
        }
    }


    public enum HighlightingMode {
        JediAndKeywords,
        JediOnly,
        KeywordsOnly,
        None,
    }

    public enum HighlightingPerformanceMode {
        Normal,
        Performance,
    }
}
