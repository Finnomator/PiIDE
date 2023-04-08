using FontAwesome.WPF;
using PiIDE.Wrapers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Editor.Parts;

public partial class CompletionUiList {

    public JediWraper.ReturnClasses.Completion? SelectedCompletion => (JediWraper.ReturnClasses.Completion?) MainListBox.SelectedItem;
    public int CompletionsCount => MainListBox.Items.Count;
    public bool SelectedAnIndex => MainListBox.SelectedIndex >= 0;
    public bool IsOpen => IsVisible;

    public EventHandler<JediWraper.ReturnClasses.Completion>? CompletionClicked;

    public bool IsBusy { get; private set; }
    private bool GotNewerRequest;

    private readonly TextEditor Editor;
    private string EditorText => Editor.TextEditorTextBox.Text;
    private bool CalledClose;
    private readonly LoadingState Loading = new();
    private readonly NoSuggestionsState NoSuggestions = new();
    private readonly Stopwatch Sw = new();

    public CompletionUiList(TextEditor editor) {
        InitializeComponent();
        Editor = editor;
        ShowActivated = false;
        ShowInTaskbar = false;

        Application.Current.MainWindow.Closed += delegate {
            base.Close();
        };
    }

    private void AddCompletions(JediWraper.ReturnClasses.Completion[] completions) {
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

        if (Tools.UpdateStats && Tools.StatsWindow != null) {
            Sw.Restart();
            await ReloadCompletions(selectFirst);
            Sw.Stop();
            Tools.StatsWindow.AddCompletionStat(Sw.ElapsedMilliseconds);
        } else
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

        JediWraper.Script script = await JediWraper.Script.MakeScriptAsync(code, filePath);
        JediWraper.ReturnClasses.Completion[] completions = await script.Complete(row, col);

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

        MainListBox.ScrollIntoView(SelectedCompletion!);
    }

    public void MoveSelectedCompletionDown() {

        if (MainListBox.Items.Count == 0)
            return;

        if (MainListBox.SelectedIndex == CompletionsCount - 1)
            MainListBox.SelectedIndex = 0;
        else
            ++MainListBox.SelectedIndex;

        MainListBox.ScrollIntoView(SelectedCompletion!);
    }

    private void Completion_Click(object sender, RoutedEventArgs e) {
        string clickedName = ((TextBlock) ((StackPanel) ((Button) sender).Content).Children[2]).Text;
        foreach (JediWraper.ReturnClasses.Completion completion in MainListBox.Items) {
            if (completion.Name == clickedName) {
                CompletionClicked?.Invoke(sender, completion);
                break;
            }
        }
        Close();
    }

    private void SetIntoLoadingState() {
        MainListBox.SelectedIndex = -1;
        MainBorder.Child = Loading;
        Show();
    }

    private void SetIntoNoSuggestionsState() {
        MainListBox.SelectedIndex = -1;
        MainBorder.Child = NoSuggestions;
    }

    private void ResetToNormalState() => MainBorder.Child = MainListBox;

    private void SelectFirst() {
        if (MainListBox.Items.Count > 0)
            MainListBox.SelectedIndex = 0;
    }

    private sealed class LoadingState : Border {
        public LoadingState() {
            BorderThickness = new(1);
            BorderBrush = (Brush) Application.Current.Resources["SplitterBackgroundBrush"];
            Background = (Brush) Application.Current.Resources["EditorBackgroundBrush"];

            WrapPanel wrapPanel = new();

            wrapPanel.Children.Add(new FontAwesome.WPF.FontAwesome { Icon = FontAwesomeIcon.Spinner, Spin = true, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White });
            wrapPanel.Children.Add(new TextBlock { Text = "Loading...", Foreground = Brushes.White });

            Child = wrapPanel;
        }
    }

    private sealed class NoSuggestionsState : Border {
        public NoSuggestionsState() {
            BorderThickness = new(1);
            BorderBrush = (Brush) Application.Current.Resources["SplitterBackgroundBrush"];
            Background = (Brush) Application.Current.Resources["EditorBackgroundBrush"];
            Child = new TextBlock { Text = "No Suggestions", Foreground = Brushes.White };
        }
    }
}