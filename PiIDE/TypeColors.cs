using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace PiIDE {

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
        public static readonly FontAwesome.WPF.FontAwesome Namespace = new() {
            Icon = FontAwesome.WPF.FontAwesomeIcon.Cubes
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
            { "namespace", Namespace },
        };

        public static FontAwesome.WPF.FontAwesome TypeToIcon(string type) {
            if (TypeToIconMap.TryGetValue(type, out FontAwesome.WPF.FontAwesome value))
                return value;
# if DEBUG
            MessageBox.Show($"Type '{type}' not found");
# endif
            return EverythingElse;
        }
    }

    public static class PylintMessageColors {

        public static Brush Fatal => (Brush) Application.Current.Resources["FatalColor"];
        public static Brush Error => (Brush) Application.Current.Resources["ErrorColor"];
        public static Brush Warning => (Brush) Application.Current.Resources["WarningColor"];
        public static Brush Convention => (Brush) Application.Current.Resources["ConventionColor"];
        public static Brush Refactor => (Brush) Application.Current.Resources["RefactorColor"];
        public static Brush Information => (Brush) Application.Current.Resources["InformationColor"];
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
