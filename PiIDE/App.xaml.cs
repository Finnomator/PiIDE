using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace PiIDE {

    public partial class App : Application {

        public App() : base() {
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
# if DEBUG
            // TODO: Remove the reset if not needed
            GlobalSettings.Default.Reset();
#endif
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

            if (!Directory.Exists("Crashlogs"))
                Directory.CreateDirectory("Crashlogs");

            using FileStream fs = File.Create($"Crashlogs/{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.txt");
            fs.Write(new UTF8Encoding(true).GetBytes(fileContent));
        }

        
    }
}
