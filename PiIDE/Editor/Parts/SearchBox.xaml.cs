using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Editor.Parts {

    public partial class SearchBox : UserControl {

        public event EventHandler<Regex>? SearchChanged;

        public SearchBox() {
            InitializeComponent();
        }

        public void Open() {
            Visibility = Visibility.Visible;
        }

        public void Close() {
            Visibility = Visibility.Collapsed;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            RegexTextBox.Text = SearchTextBox.Text;
        }

        private void RegexTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            string text = RegexTextBox.Text;
            Regex regex;
            try {
                regex = new(text);
                RegexTextBox.Background = (Brush) Application.Current.Resources["PanelBackground"];
            } catch (ArgumentException) {
                RegexTextBox.Background = Brushes.IndianRed;
                return;
            }
            SearchChanged?.Invoke(this, regex);
        }
    }
}
