using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows.Media;
using Point = System.Drawing.Point;
using CommunityToolkit.HighPerformance;
using System.Security.Policy;

namespace PiIDE {
    public static class Tools {
        public readonly static string[] PythonKeywords = new string[] {
            "False", "None", "True", "and", "await", "async", "as", "assert", "break", "class", "continue", "def", "del", "elif", "else", "except", "finally", "for", "from", "global", "if", "import", "in", "is", "lambda", "nonlocal", "not", "or", "pass", "raise", "return", "try", "while", "with", "yield"
        };
        public readonly static HashSet<string> PythonKeywordsSet = PythonKeywords.ToHashSet();
        public readonly static FontFamily CascadiaCodeFont = new("Cascadia Code");

        public readonly static string[] PythonExtensions = new string[] { ".py", ".pyi" };
        public static bool IsPythonFile(string filePath) => PythonExtensions.Contains(Path.GetExtension(filePath));
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

        public static int GetRowOfIndex(string text, int index) => CountLines(text[..index]) - 1;
        public static int CountLines(string text) => text.AsSpan().Count('\n') + 1;

        public static (int col, int row)[] GetPointsOfindexes(string text, int[] indexes) {

            // indexes must be sorted ascending

            (int col, int row)[] points = new (int col, int row)[indexes.Length];

            int col = 0;
            int row = 0;

            int index = indexes[0];
            for (int i = 0, j = 1; j < indexes.Length; i++) {

                if (i == index) {
                    points[j - 1] = (col, row);
                    index = indexes[j++];
                }

                if (text[i] == '\n') {
                    col = 0;
                    ++row;
                } else
                    ++col;
            }

            return points;
        }

        public static (int col, int row) GetPointOfIndex(string text, int index) {

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

        public static int GetIndexOfColRow(string text, int row, int col) {
            // TODO: Check if this works proberly
            string[] lines = text.Split("\r\n");
            int index = 0;
            for (int i = 0; i < row; ++i)
                index += lines[i].Length;
            index += col;
            return index;
        }
        public static int GetLengthOfLine(string text, int line) {
            string[] lines = text.Split("\r\n");
            return lines[line].Length;
        }

        public static int[] GetCOMPorts() => SerialPort.GetPortNames().Select(x => int.Parse(x[3..])).ToArray();
        public static bool IsValidCOMPort(int comPort) => GetCOMPorts().Contains(comPort);

        public static bool EnableBoardInteractions => IsValidCOMPort(GlobalSettings.Default.SelectedCOMPort) && GlobalSettings.Default.AmpyIsUsable;

        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private static readonly Random random = new();

        public static string GenerateRandomString(int length) => new(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());

        public static readonly FontAwesome.WPF.FontAwesome FontAwesome_Loading = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.Spinner, Spin = true, VerticalAlignment = VerticalAlignment.Center };

        public static readonly Thickness ZeroThichness = new(0);
    }
}
