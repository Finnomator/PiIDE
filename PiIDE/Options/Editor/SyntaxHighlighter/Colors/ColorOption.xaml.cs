using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
