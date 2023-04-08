using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace PiIDE;

public partial class App {

    public App() {
        // GlobalSettings.Default.Reset();
        Dispatcher.UnhandledException += OnDispatcherUnhandledException;

        // TODO: i think this doesn't quite work
        if (GlobalSettings.Default.CallUpgrade) {
            GlobalSettings.Default.Upgrade();
            GlobalSettings.Default.CallUpgrade = false;
        }

        Debug.WriteLine(TypeIcons.Instance);

        GlobalSettings.Default.LastOpenedBoardFilePaths ??= new();

        GlobalSettings.Default.LastOpenedLocalFilePaths ??= new();

        GlobalSettings.Default.LastOpenedLocalFolderPaths ??= new();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
        MessageBox.Show($"Unhandled exception occurred:\r\n{e.Exception.Message}\r\nYou can report this issue here: https://github.com/Finnomator/PiIDE/issues/new",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        DumpUnhandledException(e);
    }

    private static void DumpUnhandledException(DispatcherUnhandledExceptionEventArgs e) {
        string fileContent = "Exception: " + e.Exception.GetType()
                                           + "\r\nInnerException: " + e.Exception.InnerException
                                           + "\r\nMessage: " + e.Exception.Message
                                           + "\r\nStackTrace: " + e.Exception.StackTrace;

        if (!Directory.Exists("Crash-logs"))
            Directory.CreateDirectory("Crash-logs");

        using FileStream fs = File.Create($"Crash-logs/{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.txt");
        fs.Write(new UTF8Encoding(true).GetBytes(fileContent));
    }
}