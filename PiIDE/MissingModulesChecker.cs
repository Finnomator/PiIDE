using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return TryToStartProcess("python");
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

        public static bool TryToStartProcess(string fileName) {
            Process process = new() {
                StartInfo = new() {
                    FileName = fileName,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            try {
                process.Start();
            } catch {
                process.Dispose();
                return false;
            }

            process.Kill();
            process.Dispose();
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
    }


    public enum Module {
        Python,
        Ampy,
        Jedi,
    }
}
