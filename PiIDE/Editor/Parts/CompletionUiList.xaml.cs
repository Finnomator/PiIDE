using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Completion = PiIDE.Wrapers.JediWraper.ReturnClasses.Completion;
using Script = PiIDE.Wrapers.JediWraper.Script;

namespace PiIDE {

    public partial class CompletionUiList : UserControl {

        private readonly string FilePath;
        public EventHandler<Completion>? CompletionClicked;
        private bool IsBusy;
        private bool GotNewerRequest;

        public CompletionUiList(string filePath) {
            InitializeComponent();
            FilePath = filePath;
            Close();
        }

        public Completion? SelectedCompletion => (Completion?) MainListBox.SelectedItem;
        public int CompletionsCount => MainListBox.Items.Count;
        public bool SelectedAnIndex => MainListBox.SelectedIndex >= 0;
        public bool IsOpen => MainPopup.IsOpen;

        private void AddCompletions(Completion[] completions) {
            MainListBox.ItemsSource = completions;
            MainPopup.IsOpen = true;
        }

        private void ClearCompletions() => MainListBox.ItemsSource = null;

        public async void ReloadCompletionsAsync(string code, (int col, int row) caretPosition, bool selectFirst) {

            if (IsBusy) {
                GotNewerRequest = true;
                return;
            }

            IsBusy = true;

            SetIntoLoadingState();

            Script script = await Script.MakeScript(code, FilePath);
            Completion[] completions = await script.Complete(caretPosition.row, caretPosition.col);

            if (completions.Length == 0) {
                SetIntoNoSuggestionsState();
                return;
            }

            ResetToNormalState();

            AddCompletions(completions);

            if (selectFirst)
                SelectFirst();

            IsBusy = false;

            if (GotNewerRequest) {
                ReloadCompletionsAsync(code, caretPosition, selectFirst);
                GotNewerRequest = false;
            }
        }

        public void Close() {
            ClearCompletions();
            MainPopup.IsOpen = false;
        }

        public void CloseTemporary() => MainPopup.IsOpen = false;

        public void LoadCached() {
            if (MainListBox.Items.Count > 0)
                MainPopup.IsOpen = true;
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
            MainPopup.Child = new LoadingState();
            MainPopup.IsOpen = true;
        }

        private void SetIntoNoSuggestionsState() {
            MainPopup.Child = new NoSuggestionsState();
            MainPopup.IsOpen = true;
        }

        private void ResetToNormalState() {
            MainPopup.Child = MainListBox;
        }

        private void SelectFirst() {
            if (MainListBox.Items.Count > 0)
                MainListBox.SelectedIndex = 0;
        }

        private class LoadingState : Border {
            public LoadingState() {
                BorderThickness = new(1);
                BorderBrush = Brushes.Black;
                Background = Brushes.White;

                WrapPanel wrapPanel = new();

                wrapPanel.Children.Add(new FontAwesome.WPF.FontAwesome() { Icon = FontAwesome.WPF.FontAwesomeIcon.Spinner, Spin = true, VerticalAlignment = VerticalAlignment.Center });
                wrapPanel.Children.Add(new TextBlock() { Text = "Loading..." });

                Child = wrapPanel;
            }
        }

        private class NoSuggestionsState : Border {
            public NoSuggestionsState() {
                BorderThickness = new(1);
                BorderBrush = Brushes.Black;
                Background = Brushes.White;
                Child = new TextBlock() { Text = "No Suggestions" };
            }
        }
    }
}
