﻿using Humanizer;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PiIDE.Options.Editor;

public partial class Editor {

    private readonly Regex NumberChecker = new("[^0-9]+", RegexOptions.Compiled);

    public Editor() {
        InitializeComponent();

        GlobalSettings settings = GlobalSettings.Default;

        int fontIndex = 0;
        ComboBoxFontItem[] comboBoxFontItems = new ComboBoxFontItem[Tools.MonospaceFonts.Length];
        for (int i = 0; i < Tools.MonospaceFonts.Length; i++) {
            FontFamily font = Tools.MonospaceFonts[i];
            comboBoxFontItems[i] = new(font);
            if (font.Source == settings.TextEditorFontFamily)
                fontIndex = i;
        }

        FontSizeTextBox.Text = settings.TextEditorFontSize.ToString();
        EditorFontComboBox.ItemsSource = comboBoxFontItems;
        EditorFontComboBox.SelectedIndex = fontIndex;
        CompletionsComboBox.SelectedIndex = settings.CompletionsMode;
    }

    private void FontSizeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = NumberChecker.IsMatch(e.Text);

    private void UserControl_Unloaded(object sender, RoutedEventArgs e) {

    }

    private void EditorFontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => GlobalSettings.Default.TextEditorFontFamily = ((ComboBoxFontItem) EditorFontComboBox.SelectedItem).FontFamily.Source;

    private void FontSizeTextBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (int.TryParse(FontSizeTextBox.Text, out int p))
            GlobalSettings.Default.TextEditorFontSize = p;
    }

    private void ShowStatistics_Click(object sender, RoutedEventArgs e) {
        if (Tools.StatsWindow == null) {
            Tools.StatsWindow = new();
            Tools.StatsWindow.Show();
        } else {
            Tools.StatsWindow.Activate();
        }
        Tools.UpdateStats = true;
    }
}

public class ComboBoxFontItem {
    public FontFamily FontFamily { get; }
    public string Content { get; private init; }

    public ComboBoxFontItem(FontFamily fontFamily) {
        FontFamily = fontFamily;
        Content = FontFamily.Source.Humanize(LetterCasing.Title);
    }
}