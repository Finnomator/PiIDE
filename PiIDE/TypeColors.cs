using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace PiIDE {
    public static class TypeColors {

        // TODO: Make these colors customizable
        public static Brush Keyword => (Brush) Application.Current.Resources["KeywordColor"];
        public static Brush Class => (Brush) Application.Current.Resources["ClassColor"];
        public static Brush Function => (Brush) Application.Current.Resources["FunctionColor"];
        public static Brush Instance => (Brush) Application.Current.Resources["InstanceColor"]; // What is this?
        public static Brush Statement => (Brush) Application.Current.Resources["StatementColor"];
        public static Brush Module => (Brush) Application.Current.Resources["ModuleColor"];
        public static Brush Param => (Brush) Application.Current.Resources["ParamColor"];
        public static Brush Property => (Brush) Application.Current.Resources["PropertyColor"]; // What is this?
        public static Brush Path => (Brush) Application.Current.Resources["PathColor"]; // What is this?
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

        public static readonly FontAwesome.WPF.FontAwesome Keyword = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.AlignLeft };
        public static readonly FontAwesome.WPF.FontAwesome Class = new() {
            Icon = FontAwesome.WPF.FontAwesomeIcon.Cubes
        };
        public static readonly FontAwesome.WPF.FontAwesome Function = new() {
            Icon = FontAwesome.WPF.FontAwesomeIcon.Cube
        };
        public static readonly FontAwesome.WPF.FontAwesome Instance = new() {
            Icon = FontAwesome.WPF.FontAwesomeIcon.StarOutline
        };
        public static readonly FontAwesome.WPF.FontAwesome Statement = new() {
            Icon = FontAwesome.WPF.FontAwesomeIcon.Diamond
        };
        public static readonly FontAwesome.WPF.FontAwesome Module = new() {
            Icon = FontAwesome.WPF.FontAwesomeIcon.Linode
        };
        public static readonly FontAwesome.WPF.FontAwesome Param = new() {
            Icon = FontAwesome.WPF.FontAwesomeIcon.Chain
        };
        public static readonly FontAwesome.WPF.FontAwesome Property = new() {
            Icon = FontAwesome.WPF.FontAwesomeIcon.Wrench
        };
        public static readonly FontAwesome.WPF.FontAwesome Path = new() {
            Icon = FontAwesome.WPF.FontAwesomeIcon.Close
        };
        public static readonly FontAwesome.WPF.FontAwesome EverythingElse = new() {
            Icon = FontAwesome.WPF.FontAwesomeIcon.Question
        };

        private static readonly Dictionary<string, FontAwesome.WPF.FontAwesome> TypeToIconMap = new() {
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

        public static FontAwesome.WPF.FontAwesome TypeToIcon(string type) {
            if (TypeToIconMap.ContainsKey(type))
                return TypeToIconMap[type];
# if DEBUG
            MessageBox.Show($"Type '{type}' not found");
# endif
            return EverythingElse;
        }
    }

    public static class PylintMessageColors {

        public static readonly Brush Fatal = Brushes.DarkRed;
        public static readonly Brush Error = Brushes.Red;
        public static readonly Brush Warning = Brushes.Gold;
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

        public static readonly FontAwesome.WPF.FontAwesome Fatal = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.Close };
        public static readonly FontAwesome.WPF.FontAwesome Error = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.Close };
        public static readonly FontAwesome.WPF.FontAwesome Warning = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.Warning };
        public static readonly FontAwesome.WPF.FontAwesome Convention = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.InfoCircle };
        public static readonly FontAwesome.WPF.FontAwesome Refactor = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.InfoCircle };
        public static readonly FontAwesome.WPF.FontAwesome Information = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.InfoCircle };
        public static readonly FontAwesome.WPF.FontAwesome EverythingElse = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.QuestionCircle };

        private static readonly Dictionary<string, FontAwesome.WPF.FontAwesome> TypeToIconMap = new() {
            { "fatal", Fatal },
            { "error", Error},
            { "warning", Warning },
            { "convention", Convention },
            { "refactor", Refactor },
            { "information", Information },
        };

        public static FontAwesome.WPF.FontAwesome MessageTypeToIcon(string type) {
            if (TypeToIconMap.TryGetValue(type, out FontAwesome.WPF.FontAwesome? value))
                return value;
# if DEBUG
            MessageBox.Show($"Type '{type}' not found");
# endif
            return EverythingElse;
        }
    }
}
