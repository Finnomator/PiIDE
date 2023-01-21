using System.Diagnostics;

namespace PiIDE {
    internal static class PygmentizerWraper {

        public const string PygmentizePath = "Assets/Pygmentize/pygmentize.exe";



        public static string GetPygmentizedString(string filePath) {

            Process process = new() {
                StartInfo = new ProcessStartInfo() {
                    FileName = PygmentizePath,
                    Arguments = $"{filePath} -f html -O full",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }
    }
}
