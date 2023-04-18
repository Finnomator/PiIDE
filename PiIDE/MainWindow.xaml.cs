using FontAwesome.WPF;
using PiIDE.Options.Editor;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace PiIDE;

public partial class MainWindow {

    private readonly FontAwesome.WPF.FontAwesome MaximizeIcon = new() { Icon = FontAwesomeIcon.WindowMaximize };
    private readonly FontAwesome.WPF.FontAwesome RestoreIcon = new() { Icon = FontAwesomeIcon.WindowRestore };
    private Rect DraggableRect => new(0, 0, ActualWidth, TitleBarRow.ActualHeight);

    public MainWindow() {
        InitializeComponent();
        MissingModulesChecker.CheckForUsableModules();
    }

    private void Maximize_Click(object sender, RoutedEventArgs e) {

        if (WindowState == WindowState.Normal) {
            MaximizeWindow();
        } else if (WindowState == WindowState.Maximized) {
            NormalizeWindow();
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
        Point relativeMousePos = e.GetPosition(TitleBarRow);

        if (e.ChangedButton == MouseButton.Left && DraggableRect.Contains(relativeMousePos)) {
            if (WindowState != WindowState.Normal) {
                Point absoluteMousePos = PointToScreen(relativeMousePos).ConvertToDevice();
                NormalizeWindow();
                Left = absoluteMousePos.X - ActualWidth / 2;
                Top = absoluteMousePos.Y - TitleBarRow.ActualHeight / 2;
            }

            DragMove();
        }
    }

    private void MaximizeWindow() {
        WindowState = WindowState.Maximized;
        MaximizeButton.Content = RestoreIcon;
        PaddingBorder.BorderThickness = new(8);
    }

    private void NormalizeWindow() {
        WindowState = WindowState.Normal;
        MaximizeButton.Content = MaximizeIcon;
        PaddingBorder.BorderThickness = new(2);
    }

    private void Window_StateChanged(object sender, EventArgs e) {
        // Because the window can maximize without clicking the maximize button
        if (WindowState == WindowState.Maximized)
            MaximizeWindow();
    }

    private async void MainWindow_OnClosing(object? sender, CancelEventArgs e) {
        Editor.StopAllEditorAutoSaving();
        await Editor.SaveAllFilesAsync();
        GlobalSettings.Default.Save();
    }
}