using Humanizer;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PiIDE.Options.Editor {

    public partial class Editor : UserControl {

        private readonly Regex numberChecker = new("[^0-9]+", RegexOptions.Compiled);
        private readonly ComboBoxFontItem[] comboBoxFontItems;

        public Editor() {
            InitializeComponent();

            GlobalSettings settings = GlobalSettings.Default;

            int fontIndex = 0;
            comboBoxFontItems = new ComboBoxFontItem[Tools.MonospaceFonts.Length];
            for (int i = 0; i < Tools.MonospaceFonts.Length; i++) {
                FontFamily font = Tools.MonospaceFonts[i];
                comboBoxFontItems[i] = new(font);
                if (font.Source == settings.TextEditorFontFamily)
                    fontIndex = i;
            }

            FontSizeTextBox.Text = settings.TextEditorFontSize.ToString();
            EditorFontComboBox.ItemsSource = comboBoxFontItems;
            EditorFontComboBox.SelectedIndex = fontIndex;
            HighlightingModeComboBox.SelectedIndex = settings.SyntaxhighlighterMode;
            HighlighterPerformanceModeComboBox.SelectedIndex = settings.SyntaxhighlighterPerformanceMode;
            CompletionsComboBox.SelectedIndex = settings.CompletionsMode;
        }

        private void SyntaxHighlighterPerformance_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            GlobalSettings.Default.SyntaxhighlighterPerformanceMode = ((ComboBox) sender).SelectedIndex;
        }

        private void HighlightingMode_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            GlobalSettings.Default.SyntaxhighlighterMode = ((ComboBox) sender).SelectedIndex;
        }

        private void FontSizeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = numberChecker.IsMatch(e.Text);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {

        }

        private void EditorFontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            GlobalSettings.Default.TextEditorFontFamily = ((ComboBoxFontItem) EditorFontComboBox.SelectedItem).FontFamily.Source;
        }

        private void FontSizeTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (int.TryParse(FontSizeTextBox.Text, out int p))
                GlobalSettings.Default.TextEditorFontSize = p;
        }
    }

    public class ComboBoxFontItem {
        public FontFamily FontFamily { get; private init; }
        public string Content { get; private init; }

        public ComboBoxFontItem(FontFamily fontFamily) {
            FontFamily = fontFamily;
            Content = FontFamily.Source.Humanize(LetterCasing.Title);
        }
    }
}
