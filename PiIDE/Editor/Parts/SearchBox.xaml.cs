using System;
using System.Diagnostics;
using System.Linq;
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
        private SearchResultCollection? AllSearchResults;
        public bool IsOpen => MainExpander.IsExpanded;

        private bool CaseSensitive;
        private bool MatchWholeWord;
        private bool UseRegex;
        private bool GotInit;

        public HighlightingRenderer? ResultRenderBox { get; set; }
        private Size TextSize => ResultRenderBox.Editor.TextEditorTextBoxCharacterSize;

        private Brush HighlighedWordBrush = (Brush) Tools.BrushConverter.ConvertFromString("#20FFFFFF")!;
        private Brush CurrentWordBrush = (Brush) Tools.BrushConverter.ConvertFromString("#3AFFFFFF")!;

        public SearchBox() {
            InitializeComponent();
        }

        public void Initialize() {
            Debug.Assert(ResultRenderBox is not null, "ResultRenderBox must not be set");
            ResultRenderBox.TextRenderer.AddRenderAction(RenderSearchResults);
            ResultRenderBox.Editor.TextEditorTextBox.TextChanged += (s, e) => {
                Close();
            };
            GotInit = true;
        }

        public void Open() {
            Debug.Assert(GotInit, "Initialze must be called");
            MainExpander.IsExpanded = true;
        }

        public async void OpenAndFocus() {
            Open();
            while (!SearchTextBox.IsLoaded)
                await Task.Delay(10); // If we dont wait for it to load it wont get focused
            FocusSearchTextBox();
        }

        public void FocusSearchTextBox() {
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
        }

        public void Close() {
            MainExpander.IsExpanded = false;
            AllSearchResults = null;
            ResultRenderBox.TextRenderer.Render();
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void RenderSearchResults(DrawingContext context) {

            if (AllSearchResults is null)
                return;

            int fvl = ResultRenderBox.Editor.FirstVisibleLineNum;
            int lvl = ResultRenderBox.Editor.LastVisibleLineNum;

            for (int i = 0; i < AllSearchResults.Count; ++i) {
                SearchResult match = AllSearchResults[i];

                if (match.Row < fvl)
                    continue;
                if (match.Row > lvl)
                    break;

                Rect highlightRect = new(match.Column * TextSize.Width + 2, match.Row * TextSize.Height, match.Length * TextSize.Width, TextSize.Height);

                if (i == CurrentResultNo) {
                    context.DrawRectangle(CurrentWordBrush, null, highlightRect);
                    continue;
                }

                context.DrawRectangle(HighlighedWordBrush, null, highlightRect);
            }
        }

        private void SetResultNo(int resultNo) {
            CurrentResultNo = resultNo;
            SearchResult currentMatch = AllSearchResults[CurrentResultNo];
            ResultRenderBox.Editor.ScrollToPosition(currentMatch.Row, currentMatch.Column);
            ResultRenderBox.TextRenderer.Render();
            ResultNumTextBlock.Text = (CurrentResultNo + 1).ToString();
        }

        private void UpdateSearch() {

            string userSearch = SearchTextBox.Text;

            if (userSearch == "") {
                ResultsStackPanel.Visibility = Visibility.Collapsed;

                AllSearchResults = null;

                ResultRenderBox.TextRenderer.Render();
                return;
            } else {
                ResultsStackPanel.Visibility = Visibility.Visible;
            }

            Regex rx;

            try {
                rx = MakeRegex();
            } catch (ArgumentException) {
                SearchTextBox.Background = Brushes.IndianRed;
                return;
            }

            AllSearchResults = new(ResultRenderBox.EditorText, rx.Matches(ResultRenderBox.EditorText));

            if (AllSearchResults.Count == 0) {
                ResultsStackPanel.Visibility = Visibility.Collapsed;
                AllSearchResults = null;
                ResultRenderBox.TextRenderer.Render();
                return;
            }

            TotalResultsTextBlock.Text = AllSearchResults.Count.ToString();
            SearchTextBox.Background = (Brush) Application.Current.Resources["PanelBackground"];

            SetResultNo(0);

            ResultRenderBox.TextRenderer.Render();
        }

        private Regex MakeRegex() {
            string pattern = SearchTextBox.Text;

            if (UseRegex)
                return new(pattern);

            pattern = pattern.Replace("\\", "\\\\");

            if (MatchWholeWord) {
                pattern += "\\b";
                pattern = pattern.Insert(0, "\\b");
            }

            return new(pattern, CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            SearchTextBoxHint.Visibility = SearchTextBox.Text == "" ? Visibility.Visible : Visibility.Collapsed;
            UpdateSearch();
        }

        private void PreviousResButton_Click(object sender, RoutedEventArgs e) {

            if (AllSearchResults is null || AllSearchResults.Count <= 0)
                return;

            if (--CurrentResultNo < 0)
                CurrentResultNo = AllSearchResults.Count - 1;

            SetResultNo(CurrentResultNo);
        }

        private void NextResButton_Click(object sender, RoutedEventArgs e) {

            if (AllSearchResults is null || AllSearchResults.Count <= 0)
                return;

            if (++CurrentResultNo >= AllSearchResults.Count)
                CurrentResultNo = 0;

            SetResultNo(CurrentResultNo);
        }

        private void CaseSensitive_Clicked(object sender, RoutedEventArgs e) {
            CaseSensitive = !CaseSensitive;

            if (CaseSensitive) {
                CaseSensitiveButton.BorderBrush = ColorResources.AccentColorBrush;
            } else {
                CaseSensitiveButton.BorderBrush = Brushes.Transparent;
            }

            UpdateSearch();
        }

        private void MatchWholeWord_Clicked(object sender, RoutedEventArgs e) {
            MatchWholeWord = !MatchWholeWord;

            if (MatchWholeWord) {
                MatchWholeWordButton.BorderBrush = ColorResources.AccentColorBrush;
            } else {
                MatchWholeWordButton.BorderBrush = Brushes.Transparent;
            }

            UpdateSearch();
        }

        private void UseRegex_Click(object sender, RoutedEventArgs e) {
            UseRegex = !UseRegex;

            if (UseRegex) {
                UseRegexButton.BorderBrush = ColorResources.AccentColorBrush;
                MatchWholeWordButton.IsEnabled = false;
                CaseSensitiveButton.IsEnabled = false;
            } else {
                UseRegexButton.BorderBrush = Brushes.Transparent;
                MatchWholeWordButton.IsEnabled = true;
                CaseSensitiveButton.IsEnabled = true;
            }

            UpdateSearch();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape:
                    Close();
                    break;
            }
        }

        public class SearchResultCollection {

            public int Count { get; private set; }

            private readonly SearchResult[] SearchResults;

            public SearchResultCollection(string text, MatchCollection matchCollection) {
                (int col, int row)[] points = text.GetPointsOfIndexes(matchCollection.Select(x => x.Index).ToArray());
                SearchResults = new SearchResult[matchCollection.Count];

                for (int i = 0; i < matchCollection.Count; i++)
                    SearchResults[i] = new(points[i].col, points[i].row, matchCollection[i].Index, matchCollection[i].Length);

                Count = matchCollection.Count;
            }

            public SearchResult this[int index] {
                get => SearchResults[index];
            }
        }

        public readonly struct SearchResult {
            public int Column { get; init; }
            public int Row { get; init; }
            public int Index { get; init; }
            public int Length { get; init; }

            public SearchResult(int column, int row, int index, int length) {
                Column = column;
                Row = row;
                Index = index;
                Length = length;
            }
        }
    }
}
