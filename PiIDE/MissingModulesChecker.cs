using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PiIDE {



    public static class PipModules {
        public class PipModule {
            public required string Name { get; init; }
            public required string PipInstallCommand { get; init; }
            public required string CmdCommand { get; init; }
        }

        public static PipModule Ampy = new() { Name = "Ampy", PipInstallCommand = "pip install adafruit-ampy", CmdCommand = "ampy" };
        public static PipModule Pylint = new() { Name = "Pylint", PipInstallCommand = "pip install pylint", CmdCommand = "pylint" };
    }

    public static class MissingModulesChecker {

        public static readonly PipModules.PipModule[] RequiredPipModules = new PipModules.PipModule[] {
            PipModules.Ampy,
            PipModules.Pylint,
        };

        public static bool IsPythonIstanlled() {
            using Process process = new() {
                StartInfo = new() {
                    FileName = "python",
                    Arguments = "--version",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                }
            };

            try {
                process.Start();
            } catch {
                return false;
            }

            return process.StandardOutput.ReadToEnd().Contains("Python 3.");
        }

        public static List<PipModules.PipModule> FindMissingModules() {
            List<PipModules.PipModule> modules = new();

            if (!IsPythonIstanlled())
                return RequiredPipModules.ToList();

            foreach (PipModules.PipModule module in RequiredPipModules) {
                if (!TryToStartProcess(module.CmdCommand))
                    modules.Add(module);
            }

            return modules;
        }

        public static bool IsJediUsable() {
            return true;
        }

        public static bool TryToStartProcess(string fileName, string args = "") {

            using Process process = new() {
                StartInfo = new() {
                    FileName = fileName,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            try {
                process.Start();
            } catch {
                return false;
            }

            process.Kill();
            return true;
        }

        public static void CheckForUsableModules() {

            GlobalSettings.Default.PylintIsUsable = true;
            GlobalSettings.Default.PythonIsInstalled = false;
            GlobalSettings.Default.AmpyIsUsable = true;
            GlobalSettings.Default.JediIsUsable = false;

            if (IsPythonIstanlled())
                GlobalSettings.Default.PythonIsInstalled = true;
            else
                ErrorMessager.ModuleIsNotInstalled("Python", "Python was not found", "Add Python to path or install from www.python.org");

            foreach (var missingModule in FindMissingModules()) {
                ErrorMessager.ModuleIsNotInstalled(missingModule.Name, $"Python is not installed or {missingModule.Name} was not found", $"Add {missingModule.Name} to path or install with {missingModule.PipInstallCommand}");

                if (missingModule.Name == "Ampy")
                    GlobalSettings.Default.AmpyIsUsable = false;
                else if (missingModule.Name == "Pylint")
                    GlobalSettings.Default.PylintIsUsable = false;
            }
            
            if (IsJediUsable())
                GlobalSettings.Default.JediIsUsable = true;
            else
                ErrorMessager.ModuleIsNotInstalled("Jedi", "Unknown", "None");

            GlobalSettings.Default.Save();
        }
    }
}
