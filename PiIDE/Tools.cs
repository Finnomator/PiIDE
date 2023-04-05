using CommunityToolkit.HighPerformance;
using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Screen = System.Windows.Forms.Screen;

namespace PiIDE {
    public static class Tools {

        public static bool UpdateStats { get; set; }
        public static StatisticsWindow? StatsWindow { get; set; }

        public readonly static BrushConverter BrushConverter = new();

        public readonly static string[] PythonKeywords = new string[] {
            "False", "None", "True", "and", "await", "async", "as", "assert", "break", "class", "continue", "def", "del", "elif", "else", "except", "finally", "for", "from", "global", "if", "import", "in", "is", "lambda", "nonlocal", "not", "or", "pass", "raise", "return", "try", "while", "with", "yield"
        };

        public readonly static string[] PythonExtensions = new string[] { ".py", ".pyi" };
        public static bool IsPythonFile(string filePath) => IsPythonExt(Path.GetExtension(filePath));
        public static bool IsPythonExt(string ext) => PythonExtensions.Contains(ext);

        public static int GetColOfIndex(string text, int index) {
            int col = 0;

            for (int i = 0; i < index; i++) {
                if (text[i] == '\n')
                    col = 0;
                else
                    col++;
            }
            return col;
        }

        public static int GetRowOfIndex(this string text, int index) => CountLines(text[..index]) - 1;
        public static int CountLines(this string text) => text.AsSpan().Count('\n') + 1;

        public static (int col, int row)[] GetPointsOfIndexes(this string text, int[] indexes) {

            // indexes must be sorted ascending

            (int col, int row)[] points = new (int col, int row)[indexes.Length];

            int col = 0;
            int row = 0;

            for (int i = 0, j = 0; j < indexes.Length; i++) {

                while (i == indexes[j]) {
                    points[j] = (col, row);
                    ++j;
                    if (i == text.Length || j == indexes.Length)
                        return points;
                }

                if (text[i] == '\n') {
                    col = 0;
                    ++row;
                } else
                    ++col;
            }

            return points;
        }

        public static (int col, int row) GetPointOfIndex(this string text, int index) {

            int col = 0;
            int row = 0;

            for (int i = 0; i < index; i++) {
                if (text[i] == '\n') {
                    col = 0;
                    row++;
                } else
                    col++;
            }

            return (col, row);
        }

        public static int GetIndexOfColRow(this string text, int row, int col) {
            int c = 0;
            int r = 0;
            int i = 0;

            for (; i < text.Length; i++) {

                if (row == r && c == col)
                    return i;

                if (text[i] == '\n') {
                    c = 0;
                    ++r;
                } else
                    ++c;
            }

            return i - 1;
        }

        public static int[] GetIndexesOfColRows(this string text, int[] rows, int[] cols) {
            int col = 0;
            int row = 0;

            int[] indexes = new int[rows.Length];

            for (int i = 0, j = 0; j < indexes.Length; ++i) {
                while (row == rows[j] && col == cols[j]) {
                    indexes[j] = i;
                    ++j;

                    if (j == rows.Length)
                        return indexes;
                }

                if (text[i] == '\n') {
                    col = 0;
                    row++;
                } else
                    col++;
            }

            return indexes;
        }

        public static int[] GetCOMPorts() => SerialPort.GetPortNames().Select(x => int.Parse(x[3..])).ToArray();
        public static bool IsValidCOMPort(int comPort) => GetCOMPorts().Contains(comPort);

        public static bool EnableBoardInteractions => IsValidCOMPort(GlobalSettings.Default.SelectedCOMPort) && GlobalSettings.Default.AmpyIsUsable;

        public static readonly FontAwesome.WPF.FontAwesome FontAwesome_Loading = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.Spinner, Spin = true, VerticalAlignment = VerticalAlignment.Center };

        public readonly static FontFamily[] MonospaceFonts = new FontFamily[] {
            new("Cascadia Code"),
            new("Consolas"),
        };

        public static bool TryCreateFile(string filePath) {
            try {
                if (File.Exists(filePath))
                    throw new Exception("File Exists");
                File.Create(filePath).Close();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to create file", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public static bool TryCreateDirectory(string dirPath) {
            try {
                if (Directory.Exists(dirPath))
                    throw new Exception("Directory Exists");
                Directory.CreateDirectory(dirPath);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to create directory", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public static FontAwesome.WPF.FontAwesome NewWpfSpinner() => new() {
            Icon = FontAwesome.WPF.FontAwesomeIcon.Spinner,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            Spin = true,
        };

        public readonly static ColorConverter ColorConverter = new();

        public static Color ToColor(this string hex) => (Color) ColorConverter.ConvertFromString(hex);

        public static double WindowsScalingFactor {
            get {
                if (_windowsScalingFactor > 0)
                    return _windowsScalingFactor;
                (bool success, double scalingFactor) = GetWindowsScalingFactor();
                if (!success)
                    return 1.0;

                _windowsScalingFactor = scalingFactor;
                return _windowsScalingFactor;
            }
        }
        public static double _windowsScalingFactor;

        private static (bool success, double scalingFactor) GetWindowsScalingFactor() {
            PresentationSource source = PresentationSource.FromVisual(Application.Current.MainWindow);
            double dpiX;
            if (source != null && source.CompositionTarget != null)
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
            else
                return (false, -1);
            double scalingFactor = dpiX / 96.0;  // assume dpiX == dpiY
            return (true, scalingFactor);
        }

        public static Point ConvertToDevice(this Point p) => new(p.X / WindowsScalingFactor, p.Y / WindowsScalingFactor);

        public static (int width, int height) GetActiveScreenSize() {
            Screen s = Screen.FromPoint(System.Windows.Forms.Cursor.Position);
            return (s.Bounds.Width, s.Bounds.Height);
        }

        public static Brush ToBrush(this string hex) => (Brush) BrushConverter.ConvertFromString(hex)!;

        public static string GetTimestamp(this DateTime value) => value.ToString("HH:mm:ss:fff");
    }
}
