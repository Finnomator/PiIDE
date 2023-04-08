using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace PiIDE.Options;

public partial class OptionsWindow {
    public OptionsWindow() {
        InitializeComponent();
        Application.Current.MainWindow.Closed += (_, _) => Close();
        LoadSettings();
    }

    private void LoadSettings() => VersionLabel.Content = Environment.GetEnvironmentVariable("ClickOnce_CurrentVersion") ?? "Not installed";

    private void Window_Closing(object sender, CancelEventArgs e) => GlobalSettings.Default.Save();

    private void GithubIssue_RequestNavigate(object sender, RequestNavigateEventArgs e) {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void GithubRepo_RequestNavigate(object sender, RequestNavigateEventArgs e) {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

}