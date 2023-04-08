using Humanizer;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Options.Editor.SyntaxHighlighter.Colors;

public partial class ColorOption {

    private readonly string ResourceKey;
    private readonly string DefaultCode;

    public ColorOption(string resourceKey) {
        InitializeComponent();

        ResourceKey = resourceKey;
        DefaultCode = ColorResources.HighlighterColors.DefaultColors[ResourceKey].ToString();

        TypeLabel.Content = resourceKey.Humanize(LetterCasing.Title);

        ColorTextBox.Text = ColorResources.HighlighterColors.GetBrush(ResourceKey).ToString();
    }

    private void ColorTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        Brush? newBrush = null;
        try {
            newBrush = (Brush?) Tools.BrushConverter.ConvertFromString(ColorTextBox.Text);
        } catch (FormatException) { }

        if (newBrush != null) {
            ColorResources.HighlighterColors.SetBrush(ResourceKey, newBrush);
            ResetButton.IsEnabled = newBrush.ToString() != DefaultCode;
        } else
            ResetButton.IsEnabled = true;
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e) => ColorTextBox.Text = DefaultCode;
}