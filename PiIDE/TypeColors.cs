using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace PiIDE {
    public static class TypeColors {
        public static readonly Brush Keyword = (Brush) Tools.Converter.ConvertFromString("#8634eb");
        public static readonly Brush Class = (Brush) Tools.Converter.ConvertFromString("#07ab71");
        public static readonly Brush Function = (Brush) Tools.Converter.ConvertFromString("#d6cf00");
        public static readonly Brush Instance = (Brush) Tools.Converter.ConvertFromString("#0084d6");
        public static readonly Brush Statement = (Brush) Tools.Converter.ConvertFromString("#2f82b5");
        public static readonly Brush Module = (Brush) Tools.Converter.ConvertFromString("#2f7837");
        public static readonly Brush Param = (Brush) Tools.Converter.ConvertFromString("#FF0000");
        public static readonly Brush Property = (Brush) Tools.Converter.ConvertFromString("#00FF00");
        public static readonly Brush Path = (Brush) Tools.Converter.ConvertFromString("#FF0000");
        public static readonly Brush EverythingElse = (Brush) Tools.Converter.ConvertFromString("#000000");

        private static readonly Dictionary<string, Brush> TypeToColorMap = new() {
            { "keyword", Keyword },
            { "class", Class },
            { "function", Function },
            { "instance", Instance },
            { "statement", Statement },
            { "module", Module },
            { "param", Param},
            { "property", Property},
            {"path", Path },
        };

        public static Brush TypeToColor(string type) {
            if (TypeToColorMap.ContainsKey(type))
                return TypeToColorMap[type];
# if DEBUG
            MessageBox.Show($"Type '{type}' not found");
            throw new KeyNotFoundException(type);
# endif
            return EverythingElse;
        }
    }

    public static class PylintMessageColors {

        public static readonly Brush Fatal = (Brush) Tools.Converter.ConvertFromString("#FF0000");
        public static readonly Brush Error = (Brush) Tools.Converter.ConvertFromString("#FF0000");
        public static readonly Brush Warning = (Brush) Tools.Converter.ConvertFromString("#FFFF00");
        public static readonly Brush Convention = (Brush) Tools.Converter.ConvertFromString("#0000FF");
        public static readonly Brush Refactor = (Brush) Tools.Converter.ConvertFromString("#0000FF");
        public static readonly Brush Information = (Brush) Tools.Converter.ConvertFromString("#0000FF");
        public static readonly Brush EverythingElse = (Brush) Tools.Converter.ConvertFromString("#000000");

        private static readonly Dictionary<string, Brush> TypeToColorMap = new() {
            { "fatal", Fatal },
            { "error", Error },
            { "warning", Warning },
            { "convention", Convention },
            { "refactor", Refactor },
            { "information", Information },
        };

        public static Brush MessageTypeToColor(string type) {
            if (TypeToColorMap.ContainsKey(type))
                return TypeToColorMap[type];
# if DEBUG
            MessageBox.Show($"Type '{type}' not found");
            throw new KeyNotFoundException(type);
# endif
            return EverythingElse;
        }
    }
}
