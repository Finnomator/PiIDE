using System.Windows;

namespace PiIDE {
    internal static class ErrorMessager {

        public static void PromptForCOMPort() => MessageBox.Show("Select a COM port first!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        public static void ModuleIsNotInstalled(string missingModule, string reason, string solution) =>
            MessageBox.Show($"Missing module: {missingModule}\nReason: {reason}\nSolution: {solution}",
            "Warning",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);

    }
}
