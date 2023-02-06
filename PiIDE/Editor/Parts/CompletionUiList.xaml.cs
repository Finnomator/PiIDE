using PiIDE.Wrapers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Point = System.Drawing.Point;

namespace PiIDE {

    public partial class CompletionUiList : UserControl {

        private readonly string FilePath;
        public EventHandler<Completion>? CompletionClicked;

        public CompletionUiList(string filePath) {
            InitializeComponent();
            FilePath = filePath;
            Close();
        }

        public Completion SelectedCompletion => (Completion) MainListBox.SelectedItem;
        public int CompletionsCount => MainListBox.Items.Count;
        public bool SelectedAnIndex => MainListBox.SelectedIndex >= 0;
        public bool IsOpen => MainListBox.IsVisible;

        private void AddCompletions(Completion[] completions) {
            MainListBox.ItemsSource = completions;
            MainListBox.Visibility = Visibility.Visible;
        }

        private void ClearCompletions() => MainListBox.ItemsSource = null;

        public async Task ReloadCompletionsAsync(string fileContent, bool enableTypeHints, Point caretPosition) {

            SetIntoLoadingState();

            if (!JediCompletionWraper.FinishedGettingCompletions)
                return;

            Completion[] completions = await JediCompletionWraper.GetCompletionAsync(FilePath, fileContent, enableTypeHints, caretPosition);

            if (completions.Length == 0)
                return;

            AddCompletions(completions);
        }

        public void Close() {
            ClearCompletions();
            MainListBox.Visibility = Visibility.Collapsed;
        }

        public void MoveSelectedCompletionUp() {

            if (MainListBox.Items.Count == 0)
                return;

            if (MainListBox.SelectedIndex == 0)
                MainListBox.SelectedIndex = CompletionsCount - 1;
            else
                --MainListBox.SelectedIndex;

            MainListBox.ScrollIntoView(SelectedCompletion);
        }

        public void MoveSelectedCompletionDown() {

            if (MainListBox.Items.Count == 0)
                return;

            if (MainListBox.SelectedIndex == CompletionsCount - 1)
                MainListBox.SelectedIndex = 0;
            else
                ++MainListBox.SelectedIndex;

            MainListBox.ScrollIntoView(SelectedCompletion);
        }

        private void ListBox_MouseLeftButtonDown(object sender, RoutedEventArgs e) {
            CompletionClicked?.Invoke(sender, (Completion) ((ListBoxItem) sender).Content);
            Close();
        }

        private void SetIntoLoadingState() {
            Completion dumy = new() {
                Name = "Loading...",
                ForegroundColor = Brushes.Black,
                Icon = new FontAwesome.WPF.FontAwesome { Icon = FontAwesome.WPF.FontAwesomeIcon.Spinner, Spin = true },
            };
            MainListBox.ItemsSource = new Completion[] { dumy };
            MainListBox.Visibility = Visibility.Visible;
        }

        public void SelectFirst() {
            if (MainListBox.Items.Count > 0)
                MainListBox.SelectedIndex = 0;
        }
    }
}
