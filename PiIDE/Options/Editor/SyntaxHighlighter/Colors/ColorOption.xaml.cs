using Humanizer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Options.Editor.SyntaxHighlighter.Colors {

    public partial class ColorOption : UserControl {

        private readonly string ResourceKey;
        // private readonly string DefaultCode;
        private readonly HighlightingMethod HighlightingMethod;

        public ColorOption(HighlightingMethod highlightingMethod, string resourceKey) {
            InitializeComponent();

            ResourceKey = resourceKey;
            HighlightingMethod = highlightingMethod;
            // DefaultCode = ColorResources.HighlighterColors.DefaultColors[HighlightingMethod][ResourceKey].ToString();

            TypeLabel.Content = resourceKey.Humanize(LetterCasing.Title);

            ColorTextBox.Text = ColorResources.HighlighterColors.GetBrush(HighlightingMethod, ResourceKey).ToString();
        }

        private void ColorTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            Brush? newBrush = null;
            try {
                newBrush = (Brush?) Tools.BrushConverter.ConvertFromString(ColorTextBox.Text);
            } catch (FormatException) { }

            if (newBrush != null) {
                ColorResources.HighlighterColors.SetBrush(HighlightingMethod, ResourceKey, newBrush);
                // ResetButton.IsEnabled = newBrush.ToString() != DefaultCode;
            } else
                ResetButton.IsEnabled = true;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e) {
            // ColorTextBox.Text = DefaultCode;
        }
    }
}
