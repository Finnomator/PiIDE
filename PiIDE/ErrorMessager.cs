using System.Windows;

namespace PiIDE {
    internal static class ErrorMessager {

        public static void PromptForCOMPort() => MessageBox.Show("Select a COM port first!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        public static void ModuleIsNotInstalled(PipModules.PipModule module) =>

        MessageBox.Show($"Missing module: {module.Name}\n" +
            $"Reason: Python is not installed or {module.Name} was not found\n" +
            $"Solution: Add {module.Name} to path or install with {module.PipInstallCommand}",
            "Warning",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
        public static void AmpyIsBusy() => MessageBox.Show("Device is busy!\nTry again later",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        public static void FeatureNotSupported() => MessageBox.Show("This feature is currently not available",
            "Information",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
