using System.Collections.Generic;
using System.Windows.Media;

namespace PiIDE.Options.Editor.SyntaxHighlighter.Colors {
    public class Colors {

        public static Dictionary<string, Brush> JediColors { get; } = new()
        {
            {"instance", "#FF9CDCFE".ToBrush()},
            {"function", "#FFDCDCAA".ToBrush()},
            {"path", "#FF5192C5".ToBrush()},
            {"property", "#FF9CDCFE".ToBrush()},
            {"statement", "#FF9CDCFE".ToBrush()},
            {"class", "#FF4EC99D".ToBrush()},
            {"module", "#FF4EC9A2".ToBrush()},
            {"param", "#FF9CDCFE".ToBrush()},
            {"namespace", "#FF9CDCFE".ToBrush()},
            {"keyword", "#FFC586C0".ToBrush()},
            {"comment", "#FF6A9955".ToBrush()},
            {"number", "#FFB5CEA8".ToBrush()},
            {"string", "#FFCE916A".ToBrush()}
        };

        public static Dictionary<string, Brush> TokenizerColors { get; } = new()
        {
            {"NAME", "#FF9CDCFE".ToBrush()},
            {"NUMBER", "#FFB5CEA8".ToBrush()},
            {"STRING", "#FFCE916A".ToBrush()},
            {"COMMENT", "#FF6A9955".ToBrush()}
        };

        public static Dictionary<HighlightingMethod, Dictionary<string, Brush>> AllColors { get; } = new() {
            { HighlightingMethod.Jedi, JediColors },
            {HighlightingMethod.Tokenizer, TokenizerColors },
        };
    }
}
