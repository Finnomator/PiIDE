using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PiIDE.Editor.Parts {

    public partial class SearchBox : UserControl {

        public event EventHandler<Regex?>? SearchChanged;
        public event EventHandler<int>? SelectedResultChanged;
        public event EventHandler? Closed;

        public int SearchResults { get; private set; }
        public int ResultNo { get; private set; }
        public Regex? Searcher { get; private set; }
        public bool IsOpen => MainExpander.IsExpanded;

        private bool MatchWholeWord;
        private bool CaseSensitive;

        private bool BlockTextChange;

        public SearchBox() {
            InitializeComponent();
            SetSearchResults(0);
        }

        public void SetSearchResults(int searchResults) {
            SearchResults = searchResults;

            if (searchResults == 0) {
                SetResultNo(-1);
                ResultsStackPanel.Visibility = Visibility.Collapsed;
            } else {
                SetResultNo(0);
                TotalResultsTextBlock.Text = searchResults.ToString();
                ResultsStackPanel.Visibility = Visibility.Visible;
            }
        }

        public void SetResultNo(int resultNo) {
            ResultNo = resultNo;
            ResultNumTextBlock.Text = (ResultNo + 1).ToString();
        }

        public void Open() => MainExpander.IsExpanded = true;

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

        private void UpdateSearch(string text) {

            if (text == "") {
                SetSearchResults(0);
                RegexTextBoxHint.Visibility = Visibility.Visible;
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
                RegexTextBox.Background = (Brush) Application.Current.Resources["PanelBackground"];
            } catch (ArgumentException) {
                SetSearchResults(0);
                RegexTextBox.Background = Brushes.IndianRed;
            }

            SearchChanged?.Invoke(this, Searcher);
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
            if (--ResultNo < 0)
                ResultNo = SearchResults - 1;
            ResultNumTextBlock.Text = (ResultNo + 1).ToString();
            SelectedResultChanged?.Invoke(this, ResultNo);
        }

        private void NextResButton_Click(object sender, RoutedEventArgs e) {
            if (++ResultNo >= SearchResults)
                ResultNo = 0;
            ResultNumTextBlock.Text = (ResultNo + 1).ToString();
            SelectedResultChanged?.Invoke(this, ResultNo);
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
    }
}
