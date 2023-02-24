using Humanizer;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Options.Editor.SyntaxHighlighter.Colors {

    public partial class ColorOption : UserControl {

        private readonly string ResourceKey;

        public ColorOption(string resourceKey) {
            InitializeComponent();

            ResourceKey = resourceKey;

            string defaultCode = ColorResources.HighlighterColors.DefaultColors[resourceKey].ToString();

            TypeLabel.Content = resourceKey.Humanize(LetterCasing.Title);

            ResetButton.Click += delegate {
                ColorTextBox.Text = defaultCode;
            };

            ColorTextBox.TextChanged += delegate {
                Brush? newBrush = null;
                try {
                    newBrush = (Brush?) Tools.BrushConverter.ConvertFromString(ColorTextBox.Text);
                } catch (FormatException) { }

                if (newBrush != null) {
                    ColorResources.HighlighterColors.SetBrush(resourceKey, newBrush);
                    ResetButton.IsEnabled = newBrush.ToString() != defaultCode;
                } else
                    ResetButton.IsEnabled = true;
            };
            ReloadColor();
        }

        public void ReloadColor() {
            ColorTextBox.Text = ColorResources.HighlighterColors.GetBrush(ResourceKey).ToString();
        }
    }
}
