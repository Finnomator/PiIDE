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

    public static class TypeIcons {

        public const FontAwesome.WPF.FontAwesomeIcon Keyword = FontAwesome.WPF.FontAwesomeIcon.AlignLeft;
        public const FontAwesome.WPF.FontAwesomeIcon Class = FontAwesome.WPF.FontAwesomeIcon.Cubes;
        public const FontAwesome.WPF.FontAwesomeIcon Function = FontAwesome.WPF.FontAwesomeIcon.Cube;
        public const FontAwesome.WPF.FontAwesomeIcon Instance = FontAwesome.WPF.FontAwesomeIcon.Close;
        public const FontAwesome.WPF.FontAwesomeIcon Statement = FontAwesome.WPF.FontAwesomeIcon.Close;
        public const FontAwesome.WPF.FontAwesomeIcon Module = FontAwesome.WPF.FontAwesomeIcon.Close;
        public const FontAwesome.WPF.FontAwesomeIcon Param = FontAwesome.WPF.FontAwesomeIcon.Close;
        public const FontAwesome.WPF.FontAwesomeIcon Property = FontAwesome.WPF.FontAwesomeIcon.Close;
        public const FontAwesome.WPF.FontAwesomeIcon Path = FontAwesome.WPF.FontAwesomeIcon.Close;
        public const FontAwesome.WPF.FontAwesomeIcon EverythingElse = FontAwesome.WPF.FontAwesomeIcon.Close;

        private static readonly Dictionary<string, FontAwesome.WPF.FontAwesomeIcon> TypeToIconMap = new() {
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

        public static FontAwesome.WPF.FontAwesomeIcon TypeToIcon(string type) {
            if (TypeToIconMap.TryGetValue(type, out FontAwesome.WPF.FontAwesomeIcon value))
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

    public static class PylintMessageIcons {

        public const FontAwesome.WPF.FontAwesomeIcon Fatal = FontAwesome.WPF.FontAwesomeIcon.Close;
        public const FontAwesome.WPF.FontAwesomeIcon Error = FontAwesome.WPF.FontAwesomeIcon.Close;
        public const FontAwesome.WPF.FontAwesomeIcon Warning = FontAwesome.WPF.FontAwesomeIcon.Warning;
        public const FontAwesome.WPF.FontAwesomeIcon Convention = FontAwesome.WPF.FontAwesomeIcon.InfoCircle;
        public const FontAwesome.WPF.FontAwesomeIcon Refactor = FontAwesome.WPF.FontAwesomeIcon.InfoCircle;
        public const FontAwesome.WPF.FontAwesomeIcon Information = FontAwesome.WPF.FontAwesomeIcon.InfoCircle;
        public const FontAwesome.WPF.FontAwesomeIcon EverythingElse = FontAwesome.WPF.FontAwesomeIcon.QuestionCircle;

        private static readonly Dictionary<string, FontAwesome.WPF.FontAwesomeIcon> TypeToIconMap = new() {
            {"fatal", Fatal },
            { "error", Error},
            {"warning", Warning },
            {"convention", Convention },
            {"Refactor", Refactor },
            {"information", Information },
        };

        public static FontAwesome.WPF.FontAwesomeIcon MessageTypeToIcon(string type) {
            if (TypeToIconMap.TryGetValue(type, out FontAwesome.WPF.FontAwesomeIcon value))
                return value;
# if DEBUG
            MessageBox.Show($"Type '{type}' not found");
# endif
            return EverythingElse;
        }
    }
}
