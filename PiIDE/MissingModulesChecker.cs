using System.Collections.Generic;
using System.Diagnostics;

namespace PiIDE {
    public static class MissingModulesChecker {

        public static readonly string[] RequiredModules = new string[] {
            "Python",
            "Ampy",
        };

        static MissingModulesChecker() {

        }

        public static bool IsPylintUsable() {
            return IsPythonIstanlled();
        }

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

        public static bool IsAmpyInstalled() {
            return TryToStartProcess("ampy");
        }

        public static bool IsAmpyUsable() {
            return IsPythonIstanlled() && IsAmpyInstalled();
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

        public static List<Module> GetUsableModules() {
            List<Module> usableModules = new();
            if (IsAmpyUsable()) {
                usableModules.Add(Module.Python);
                usableModules.Add(Module.Ampy);
            } else if (IsPythonIstanlled())
                usableModules.Add(Module.Python);

            if (IsJediUsable())
                usableModules.Add(Module.Jedi);

            return usableModules;
        }

        public static void CheckForUsableModules() {

            GlobalSettings.Default.PylintIsUsable = false;
            GlobalSettings.Default.PythonIsInstalled = false;
            GlobalSettings.Default.AmpyIsUsable = false;
            GlobalSettings.Default.JediIsUsable = false;

            if (IsAmpyUsable()) {
                GlobalSettings.Default.PythonIsInstalled = true;
                GlobalSettings.Default.AmpyIsUsable = true;
            } else {
                if (IsPythonIstanlled())
                    GlobalSettings.Default.PythonIsInstalled = true;
                else
                    ErrorMessager.ModuleIsNotInstalled("Python", "Python was not found", "Add Python to path or install from www.python.org");

                ErrorMessager.ModuleIsNotInstalled("Ampy", "Python is not installed or Ampy was not found", "Add Ampy to path or install with pip install adafruit-ampy");
            }

            if (IsPylintUsable())
                GlobalSettings.Default.PylintIsUsable = true;
            else
                ErrorMessager.ModuleIsNotInstalled("Pylint", "Python was not found", "Add Python to path or install from www.python.org");

            if (IsJediUsable())
                GlobalSettings.Default.JediIsUsable = true;
            else
                ErrorMessager.ModuleIsNotInstalled("Jedi", "Unknown", "None");

            GlobalSettings.Default.Save();
        }
    }


    public enum Module {
        Python,
        Ampy,
        Jedi,
    }
}
