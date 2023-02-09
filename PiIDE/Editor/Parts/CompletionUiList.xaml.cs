using PiIDE.Wrapers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Point = System.Drawing.Point;
using Completion = PiIDE.Wrapers.JediWraper.ReturnClasses.Completion;
using Script = PiIDE.Wrapers.JediWraper.Script;
using static PiIDE.Wrapers.JediWraper;

namespace PiIDE {

    public partial class CompletionUiList : UserControl {

        private readonly string FilePath;
        private readonly WraperRepl Repl = new();
        public EventHandler<Completion>? CompletionClicked;

        public CompletionUiList(string filePath) {
            InitializeComponent();
            FilePath = filePath;
            Close();
        }

        public Completion? SelectedCompletion => (Completion?) MainListBox.SelectedItem;
        public int CompletionsCount => MainListBox.Items.Count;
        public bool SelectedAnIndex => MainListBox.SelectedIndex >= 0;
        public bool IsOpen => MainListBox.IsVisible;

        private void AddCompletions(Completion[] completions) {
            MainListBox.ItemsSource = completions;
            MainListBox.Visibility = Visibility.Visible;
        }

        private void ClearCompletions() => MainListBox.ItemsSource = null;

        public async Task ReloadCompletionsAsync(string code, Point caretPosition) {

            SetIntoLoadingState();

            Script script = new(Repl, code, FilePath);
            Completion[] completions = await script.Complete(caretPosition.Y, caretPosition.X);


            if (completions.Length == 0) {
                SetIntoNoSuggestionsState();
                return;
            }

            AddCompletions(completions);
        }

        public void Close() {
            ClearCompletions();
            MainListBox.Visibility = Visibility.Collapsed;
        }

        public void CloseTemporary() => MainListBox.Visibility = Visibility.Collapsed;

        public void LoadCached() {
            if (MainListBox.Items.Count > 0)
                MainListBox.Visibility = Visibility.Visible;
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
            /*
            Completion dumy = new() {
                Name = "Loading...",
                Foreground = Brushes.Black,
                Icon = Tools.FontAwesome_Loading,
            };
            MainListBox.ItemsSource = new Completion[] { dumy };
            MainListBox.Visibility = Visibility.Visible;
            */
        }

        private void SetIntoNoSuggestionsState() {
            /*
            Completion dumy = new() {
                Name = "No Suggestions",
                Foreground = Brushes.Black,
            };
            MainListBox.ItemsSource = new Completion[] { dumy };
            MainListBox.Visibility = Visibility.Visible;
            */
        }

        public void SelectFirst() {
            if (MainListBox.Items.Count > 0)
                MainListBox.SelectedIndex = 0;
        }
    }
}
