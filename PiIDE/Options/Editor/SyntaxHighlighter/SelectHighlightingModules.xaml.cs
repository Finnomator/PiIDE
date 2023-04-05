using System.Windows;
using System.Windows.Controls;

namespace PiIDE.Options.Editor.SyntaxHighlighter {

    public partial class SelectHighlightingModules : UserControl {

        public SelectHighlightingModules() {
            InitializeComponent();

            JediNamesCheckBox.IsChecked = SyntaxHighlighterSettings.Default.HighlightJediNames;
            KeywordsCheckBox.IsChecked = SyntaxHighlighterSettings.Default.HighlightKeywords;
            BracketsCheckBox.IsChecked = SyntaxHighlighterSettings.Default.HighlightBrackets;
            IndentationCheckBox.IsChecked = SyntaxHighlighterSettings.Default.HighlightIndentation;
        }

        private void JediNamesCheckBox_Click(object sender, RoutedEventArgs e) => SyntaxHighlighterSettings.Default.HighlightJediNames = (bool) JediNamesCheckBox.IsChecked!;

        private void KeywordsCheckBox_Click(object sender, RoutedEventArgs e) => SyntaxHighlighterSettings.Default.HighlightKeywords = (bool) KeywordsCheckBox.IsChecked!;

        private void BracketsCheckBox_Click(object sender, RoutedEventArgs e) => SyntaxHighlighterSettings.Default.HighlightBrackets = (bool) BracketsCheckBox.IsChecked!;

        private void IndentationCheckBox_Click(object sender, RoutedEventArgs e) => SyntaxHighlighterSettings.Default.HighlightIndentation = (bool) IndentationCheckBox.IsChecked!;

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) => SyntaxHighlighterSettings.Default.Save();
    }
}
