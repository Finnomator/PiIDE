using System;
using System.Diagnostics;
using System.Windows;

namespace PiIDE;

public partial class OptionsWindow {
    public OptionsWindow() {
        InitializeComponent();
        Application.Current.MainWindow.Closed += (_, _) => Close();
        LoadSettings();
    }

    private void LoadSettings() => VersionLabel.Content = Environment.GetEnvironmentVariable("ClickOnce_CurrentVersion") ?? "Not installed";

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => GlobalSettings.Default.Save();

    private void GithubIssue_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void GithubRepo_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

}