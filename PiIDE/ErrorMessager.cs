using System.Windows;

namespace PiIDE {
    internal static class ErrorMessager {

        public static void PromptForCOMPort() => MessageBox.Show("Select a COM port first!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
