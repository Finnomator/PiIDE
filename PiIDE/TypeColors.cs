using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace PiIDE {
    public static class TypeColors {

        // TODO: Make these colors customizable
        public static readonly Brush Keyword = Brushes.BlueViolet;
        public static readonly Brush Class = Brushes.DarkCyan;
        public static readonly Brush Function = Brushes.Peru;
        public static readonly Brush Instance = Brushes.Red; // What is this?
        public static readonly Brush Statement = Brushes.SteelBlue;
        public static readonly Brush Module = Brushes.MediumSeaGreen;
        public static readonly Brush Param = Brushes.SteelBlue;
        public static readonly Brush Property = Brushes.Red; // What is this?
        public static readonly Brush Path = Brushes.Red; // What is this?
        public static readonly Brush EverythingElse = Brushes.Gray;

        private static readonly Dictionary<string, Brush> TypeToColorMap = new() {
            { "keyword", Keyword },
            { "class", Class },
            { "function", Function },
            { "instance", Instance },
            { "statement", Statement },
            { "module", Module },
            { "param", Param},
            { "property", Property},
            { "path", Path },
        };

        public static Brush TypeToColor(string type) {
            if (TypeToColorMap.TryGetValue(type, out Brush? value))
                return value;
# if DEBUG
            MessageBox.Show($"Type '{type}' not found");
# endif
            return EverythingElse;
        }
    }

    public static class PylintMessageColors {

        public static readonly Brush Fatal = Brushes.DarkRed;
        public static readonly Brush Error = Brushes.Red;
        public static readonly Brush Warning = Brushes.Yellow;
        public static readonly Brush Convention = Brushes.Blue;
        public static readonly Brush Refactor = Brushes.LightBlue;
        public static readonly Brush Information = Brushes.CadetBlue;
        public static readonly Brush EverythingElse = Brushes.Gray;

        private static readonly Dictionary<string, Brush> TypeToColorMap = new() {
            { "fatal", Fatal },
            { "error", Error },
            { "warning", Warning },
            { "convention", Convention },
            { "refactor", Refactor },
            { "information", Information },
        };

        public static Brush MessageTypeToColor(string type) {
            if (TypeToColorMap.TryGetValue(type, out Brush? value))
                return value;
# if DEBUG
            MessageBox.Show($"Type '{type}' not found");
# endif
            return EverythingElse;
        }
    }
}
