using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace PiIDE {
    public static class Tools {
        public static readonly BrushConverter Converter = new();
        public static readonly Brush SelectedBrush = (Brush) Converter.ConvertFromString("#24000000");
        public static readonly Brush HighlightBrush = (Brush) Converter.ConvertFromString("#10000000");
        public static readonly Brush UnselectedBrush = Brushes.Transparent;
        public static bool AreKeysPressed(Key[] keys) {
            for (int i = 0; i < keys.Length; i++) {
                if (!Keyboard.IsKeyDown(keys[i]))
                    return false;
            }
            return true;
        }
        public static bool IsShortCutPressed(Key key1, Key key2) => Keyboard.IsKeyDown(key1) && Keyboard.IsKeyDown(key2);
        public readonly static string[] PythonKeywords = new string[] {
            "False", "None", "True", "and", "as", "assert", "break", "class", "continue", "def", "del", "elif", "else", "except", "finally", "for", "from", "global", "if", "import", "in", "is", "lambda", "nonlocal", "not", "or", "pass", "raise", "return", "try", "while", "with", "yield"
        };
        public readonly static HashSet<string> PythonKeywordsSet = PythonKeywords.ToHashSet();
        public readonly static FontFamily CascadiaCodeFont = new("Cascadia Code");
    }
}
