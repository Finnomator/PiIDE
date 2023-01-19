using System.Collections.Generic;
using System.Windows.Media;

namespace PiIDE {
    public static class TypeColors {
        public static readonly Brush Keyword = (Brush) Tools.Converter.ConvertFromString("#8634eb");
        public static readonly Brush Class = (Brush) Tools.Converter.ConvertFromString("#07ab71");
        public static readonly Brush Function = (Brush) Tools.Converter.ConvertFromString("#d6cf00");
        public static readonly Brush Instance = (Brush) Tools.Converter.ConvertFromString("#0084d6");
        public static readonly Brush Statement = (Brush) Tools.Converter.ConvertFromString("#2f82b5");
        public static readonly Brush Module = (Brush) Tools.Converter.ConvertFromString("#2f7837");

        public static readonly Dictionary<string, Brush> TypeToColorMap = new() {
            { "keyword", Keyword },
            { "class", Class },
            { "function", Function },
            { "instance", Instance },
            { "statement", Statement },
            { "module", Module },
        };
    }
}
