using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Editor.Parts {

    public partial class SearchBox : UserControl {

        public event EventHandler<Regex?>? SearchChanged;
        public event EventHandler<int>? SelectedResultChanged;

        public int SearchResults { get; private set; }
        public int ResultNo { get; private set; }
        public Regex? Searcher { get; private set; }

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

        public void Open() {
            MainExpander.IsExpanded = true;
        }

        public void OpenAndFocus() {
            Open();
            SearchTextBox.Focus();
        }

        public void Close() {
            MainExpander.IsExpanded = false;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            RegexTextBox.Text = SearchTextBox.Text;

            if (SearchTextBox.Text == "") {
                SearchTextBoxHint.Visibility = Visibility.Visible;
            } else {
                SearchTextBoxHint.Visibility = Visibility.Collapsed;
            }
        }

        private void RegexTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            string text = RegexTextBox.Text.Replace("\\", "\\\\");

            if (text == "") {
                Searcher = null;
                SetSearchResults(0);
                RegexTextBoxHint.Visibility = Visibility.Visible;
            } else {
                RegexTextBoxHint.Visibility = Visibility.Collapsed;
                try {
                    Searcher = new(text);
                    RegexTextBox.Background = (Brush) Application.Current.Resources["PanelBackground"];
                } catch (ArgumentException) {
                    SetSearchResults(0);
                    RegexTextBox.Background = Brushes.IndianRed;
                    return;
                }
            }

            SearchChanged?.Invoke(this, Searcher);
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
    }
}
