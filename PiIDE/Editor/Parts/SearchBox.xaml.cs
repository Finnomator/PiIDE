using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PiIDE.Editor.Parts {

    public partial class SearchBox : UserControl {

        public event EventHandler? Closed;

        public int CurrentResultNo { get; private set; }
        private Regex? Searcher;
        private MatchCollection? SearchResults;
        public bool IsOpen => MainExpander.IsExpanded;

        private bool MatchWholeWord;
        private bool CaseSensitive;

        private bool BlockTextChange;

        public HighlightingRenderer? ResultRenderBox { get; set; }

        public SearchBox() {
            InitializeComponent();
        }

        public void Open() {
            Debug.Assert(ResultRenderBox is not null, "ResultRenderBox must not be set");
            ResultRenderBox.TextRenderer.AddRenderAction(RenderSearchResults);
            MainExpander.IsExpanded = true;
        }

        public async void OpenAndFocus() {
            Open();
            while (!SearchTextBox.IsLoaded)
                await Task.Delay(10); // If we dont wait for it to load it wont get focused
            FocusSearchTextBox();
        }

        public void FocusSearchTextBox() => SearchTextBox.Focus();

        public void Close() {
            MainExpander.IsExpanded = false;
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void RenderSearchResults(DrawingContext context) {

            if (SearchResults is null)
                return;

            for (int i = 0; i < SearchResults.Count; ++i) {
                Match match = SearchResults[i];

                if (i == CurrentResultNo) {
                    ResultRenderBox.RendererFormattedText.SetForegroundBrush(Brushes.Yellow, match.Index, match.Length);
                    continue;
                }

                ResultRenderBox.RendererFormattedText.SetForegroundBrush(Brushes.Blue, match.Index, match.Length);
            }
        }

        private void SetResultNo(int resultNo) {
            CurrentResultNo = resultNo;
            ResultNumTextBlock.Text = (CurrentResultNo + 1).ToString();
        }

        private void UpdateSearch(string text) {

            if (text == "") {
                RegexTextBoxHint.Visibility = Visibility.Visible;
                SearchResults = null;
                ResultRenderBox.TextRenderer.Render();
                return;
            } else
                RegexTextBoxHint.Visibility = Visibility.Collapsed;

            string regexPattern = $"{text}";

            if (MatchWholeWord) {
                regexPattern += "\\b";
                regexPattern = regexPattern.Insert(0, "\\b");
            }

            BlockTextChange = true;
            RegexTextBox.Text = regexPattern;
            BlockTextChange = false;

            try {
                Searcher = new(regexPattern, CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                SearchResults = Searcher.Matches(ResultRenderBox.RendererFormattedText.Text);
                RegexTextBox.Background = (Brush) Application.Current.Resources["PanelBackground"];
                SetResultNo(0);
            } catch (ArgumentException) {
                RegexTextBox.Background = Brushes.IndianRed;
            }

            ResultRenderBox.TextRenderer.Render();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            RegexTextBox.Text = SearchTextBox.Text.Replace("\\", "\\\\");
            SearchTextBoxHint.Visibility = SearchTextBox.Text == "" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RegexTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (BlockTextChange)
                return;
            Searcher = null;
            UpdateSearch(RegexTextBox.Text);
        }

        private void PreviousResButton_Click(object sender, RoutedEventArgs e) {

            if (SearchResults is null || SearchResults.Count <= 0)
                return;

            if (--CurrentResultNo < 0)
                CurrentResultNo = SearchResults.Count - 1;
            ResultNumTextBlock.Text = (CurrentResultNo + 1).ToString();
        }

        private void NextResButton_Click(object sender, RoutedEventArgs e) {

            if (SearchResults is null || SearchResults.Count <= 0)
                return;

            if (++CurrentResultNo >= SearchResults.Count)
                CurrentResultNo = 0;
            ResultNumTextBlock.Text = (CurrentResultNo + 1).ToString();
        }

        private void CaseSensitive_Clicked(object sender, RoutedEventArgs e) {
            CaseSensitive = (bool) ((ToggleButton) sender).IsChecked!;
            UpdateSearch(SearchTextBox.Text);
        }

        private void MatchWholeWord_Clicked(object sender, RoutedEventArgs e) {
            MatchWholeWord = (bool) ((ToggleButton) sender).IsChecked!;
            UpdateSearch(SearchTextBox.Text);
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape:
                    Close();
                    break;
            }
        }

        public readonly struct SearchResult {
            public int Index { get; init; }
            public int Column { get; init; }
            public int Row { get; init; }

            public SearchResult(int index, int column, int row) {
                Index = index;
                Column = column;
                Row = row;
            }
        }
    }
}
