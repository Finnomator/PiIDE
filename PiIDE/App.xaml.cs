using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace PiIDE {

    public partial class App : Application {

        public App() : base() {
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            CheckForUsableModules();
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

        private static void CheckForUsableModules() {

            GlobalSettings.Default.PylintIsUsable = false;
            GlobalSettings.Default.PythonIsInstalled = false;
            GlobalSettings.Default.AmpyIsUsable = false;

            if (MissingModulesChecker.IsAmpyUsable()) {
                GlobalSettings.Default.PythonIsInstalled = true;
                GlobalSettings.Default.AmpyIsUsable = true;
            } else {
                if (MissingModulesChecker.IsPythonIstanlled())
                    GlobalSettings.Default.PythonIsInstalled = true;
                else
                    ErrorMessager.ModuleIsNotInstalled("Python", "Python was not found", "Add Python to path or install from www.python.org");

                ErrorMessager.ModuleIsNotInstalled("Ampy", "Python is not installed or Ampy was not found", "Add Ampy to path or install with pip install adafruit-ampy");
            }

            if (MissingModulesChecker.IsPylintUsable())
                GlobalSettings.Default.PylintIsUsable = true;
            else
                ErrorMessager.ModuleIsNotInstalled("Pylint", "Python was not found", "Add Python to path or install from www.python.org");

            GlobalSettings.Default.Save();
        }
    }
}
