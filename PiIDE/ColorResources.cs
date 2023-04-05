using PiIDE.Options.Editor.SyntaxHighlighter.Colors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Colors = PiIDE.Options.Editor.SyntaxHighlighter.Colors.Colors;

namespace PiIDE {
    public static class ColorResources {

        public static readonly Brush AccentColorBrush = (Brush) Application.Current.Resources["AccentBrush"];

        public static class HighlighterColors {

            public static event EventHandler? ColorChanged;

            public static readonly Brush Default = Brushes.White;

            public static Brush GetBrush(HighlightingMethod highlightingMethod, string key) {
                if (Colors.AllColors[highlightingMethod].TryGetValue(key, out Brush? value))
                    return value;
                return Default;
            }

            public static void SetBrush(HighlightingMethod highlightingMethod, string key, Brush value) {
                ColorChanged?.Invoke(null, EventArgs.Empty);
                Colors.AllColors[highlightingMethod][key] = value;
            }
        }
    }
}
