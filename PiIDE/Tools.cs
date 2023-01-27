using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows.Media;

namespace PiIDE {
    public static class Tools {
        public static readonly BrushConverter Converter = new();
        public static readonly Brush SelectedBrush = (Brush) Converter.ConvertFromString("#24000000");
        public static readonly Brush HighlightBrush = (Brush) Converter.ConvertFromString("#10000000");
        public static readonly Brush UnselectedBrush = Brushes.Transparent;

        public readonly static string[] PythonKeywords = new string[] {
            "False", "None", "True", "and", "await", "async", "as", "assert", "break", "class", "continue", "def", "del", "elif", "else", "except", "finally", "for", "from", "global", "if", "import", "in", "is", "lambda", "nonlocal", "not", "or", "pass", "raise", "return", "try", "while", "with", "yield"
        };
        public readonly static HashSet<string> PythonKeywordsSet = PythonKeywords.ToHashSet();
        public readonly static FontFamily CascadiaCodeFont = new("Cascadia Code");

        public readonly static string[] PythonExtensions = new string[] { ".py", ".pyi" };
        public static bool IsPythonFile(string filePath) => PythonExtensions.Contains(Path.GetExtension(filePath));
        public static bool IsPythonExt(string ext) => PythonExtensions.Contains(ext);

        public static int GetColOfIndex(string text, int index) {
            int caretLine = GetRowOfIndex(text, index);

            int offset = 0;
            string[] lines = text.Split("\r\n");

            for (int i = 0; i < caretLine; ++i)
                offset += lines[i].Length + 2;

            return index - offset;
        }
        public static int GetRowOfIndex(string text, int index) {

            int offset = 0;
            string[] lines = text.Split("\r\n");
            int line = 0;

            for (; index >= offset; ++line)
                offset += lines[line].Length + 2;

            return line - 1;
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

    }
}
