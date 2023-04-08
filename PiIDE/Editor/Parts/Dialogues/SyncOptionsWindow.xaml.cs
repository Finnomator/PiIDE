using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace PiIDE.Editor.Parts.Dialogues;

public partial class SyncOptionsWindow {

    public SyncOptionResult SyncOptionResult { get; private set; }

    private bool OkClose;

    public SyncOptionsWindow() {
        InitializeComponent();

        Loaded += delegate {
            SyncOptionResult = SyncOptionResult.OverwriteAllLocalFiles;
            Point mousePos = PointToScreen(Mouse.GetPosition(this)).ConvertToDevice();
            (int sw, _) = Tools.GetActiveScreenSize();
            double left = mousePos.X - ActualWidth / 2;

            if (left < 0 && left > -ActualWidth / 2)
                left = 0;
            else if (left + ActualWidth > sw && left < sw + ActualWidth / 2)
                left = sw - ActualWidth;

            Left = left;
            Top = mousePos.Y;
        };

        Show();
        Focus();
    }

    private void OverwriteAllLocalFile_Checked(object sender, RoutedEventArgs e) => SyncOptionResult = SyncOptionResult.OverwriteAllLocalFiles;

    private void Ok_Click(object sender, RoutedEventArgs e) {
        OkClose = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) {
        SyncOptionResult = SyncOptionResult.Cancel;
        Close();
    }

    private void Window_Closing(object sender, CancelEventArgs e) {
        if (OkClose) {

        } else {
            SyncOptionResult = SyncOptionResult.Cancel;
        }
    }
}

public enum SyncOptionResult {
    OverwriteAllLocalFiles,
    Cancel,
}