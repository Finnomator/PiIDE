using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiIDE {
    internal static class PygmentizerWraper {

        public const string PygmentizePath = "Assets/Pygmentize/pygmentize.exe";

        public static string GetPygmentizedString(string filePath) {

            Process process = new() {
                StartInfo = new ProcessStartInfo() {
                    FileName = PygmentizePath,
                    Arguments = $"{filePath} -f html",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
# if DEBUG
            string err = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(err))
                throw new Exception(err);
#endif
            process.WaitForExit();
            return output;
        }
    }
}
