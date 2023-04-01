using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Completion = PiIDE.Wrapers.JediWraper.ReturnClasses.Completion;
using Script = PiIDE.Wrapers.JediWraper.Script;

namespace PiIDE {

    public partial class CompletionUiList : Window {

        public Completion? SelectedCompletion => (Completion?) MainListBox.SelectedItem;
        public int CompletionsCount => MainListBox.Items.Count;
        public bool SelectedAnIndex => MainListBox.SelectedIndex >= 0;
        public bool IsOpen => IsVisible;

        public EventHandler<Completion>? CompletionClicked;

        public bool IsBusy { get; private set; }
        private bool GotNewerRequest;

        private readonly TextEditor Editor;
        private string EditorText => Editor.TextEditorTextBox.Text;
        private bool CalledClose;
        private readonly LoadingState loadingState = new();
        private readonly NoSuggestionsState noSuggestionsState = new();

        public CompletionUiList(TextEditor editor) {
            InitializeComponent();
            Editor = editor;
            ShowActivated = false;
            ShowInTaskbar = false;

            Application.Current.MainWindow.Closed += delegate {
                base.Close();
            };
        }

        private void AddCompletions(Completion[] completions) {
            MainListBox.ItemsSource = completions;
            Show();
        }

        private void ClearCompletions() => MainListBox.ItemsSource = null;

        public async void ReloadCompletionsAsync(bool selectFirst) {

            if (IsBusy) {
                GotNewerRequest = true;
                return;
            }

            IsBusy = true;

            await ReloadCompletions(selectFirst);

            IsBusy = false;

            if (GotNewerRequest) {
                GotNewerRequest = false;
                ReloadCompletionsAsync(selectFirst);
            }
        }

        private async Task ReloadCompletions(bool selectFirst) {

            SetIntoLoadingState();

            string code = EditorText;
            string filePath = Editor.FilePath;
            (int col, int row) = Editor.GetCaretPosition();
            row++;

            Script script = await Script.MakeScriptAsync(code, filePath);
            Completion[] completions = await script.Complete(row, col);

            if (CalledClose)
                return;

            if (completions.Length == 0) {
                SetIntoNoSuggestionsState();
                return;
            }

            ResetToNormalState();

            AddCompletions(completions);

            if (selectFirst)
                SelectFirst();
        }

        public new void Show() {
            CalledClose = false;
            if (!IsOpen)
                base.Show();
        }

        public new void Close() {
            ClearCompletions();
            CloseTemporary();
        }

        public void CloseTemporary() {
            CalledClose = true;
            if (IsOpen)
                Hide();
        }

        public void LoadCached() {
            if (MainListBox.Items.Count > 0)
                Show();
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

        private void Completion_Click(object sender, RoutedEventArgs e) {
            string clickedName = ((TextBlock) ((StackPanel) ((Button) sender).Content).Children[2]).Text;
            foreach (Completion completion in MainListBox.Items) {
                if (completion.Name == clickedName) {
                    CompletionClicked?.Invoke(sender, completion);
                    break;
                }
            }
            Close();
        }

        private void SetIntoLoadingState() {
            MainListBox.SelectedIndex = -1;
            MainBorder.Child = loadingState;
            Show();
        }

        private void SetIntoNoSuggestionsState() {
            MainListBox.SelectedIndex = -1;
            MainBorder.Child = noSuggestionsState;
        }

        private void ResetToNormalState() => MainBorder.Child = MainListBox;

        private void SelectFirst() {
            if (MainListBox.Items.Count > 0)
                MainListBox.SelectedIndex = 0;
        }

        private class LoadingState : Border {
            public LoadingState() {
                BorderThickness = new(1);
                BorderBrush = (Brush) Application.Current.Resources["SplitterBackgroundBrush"];
                Background = (Brush) Application.Current.Resources["EditorBackgroundBrush"];

                WrapPanel wrapPanel = new();

                wrapPanel.Children.Add(new FontAwesome.WPF.FontAwesome() { Icon = FontAwesome.WPF.FontAwesomeIcon.Spinner, Spin = true, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White });
                wrapPanel.Children.Add(new TextBlock() { Text = "Loading...", Foreground = Brushes.White });

                Child = wrapPanel;
            }
        }

        private class NoSuggestionsState : Border {
            public NoSuggestionsState() {
                BorderThickness = new(1);
                BorderBrush = (Brush) Application.Current.Resources["SplitterBackgroundBrush"];
                Background = (Brush) Application.Current.Resources["EditorBackgroundBrush"];
                Child = new TextBlock() { Text = "No Suggestions", Foreground = Brushes.White };
            }
        }
    }
}
