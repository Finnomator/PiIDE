﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Completion = PiIDE.Wrapers.JediWraper.ReturnClasses.Completion;
using Script = PiIDE.Wrapers.JediWraper.Script;

namespace PiIDE {

    public partial class CompletionUiList : UserControl {

        public EventHandler<Completion>? CompletionClicked;
        private bool IsBusy;
        private bool GotNewerRequest;

        private readonly TextEditor Editor;
        private TextBox EditorBox => Editor.TextEditorTextBox;
        private string EditorText => Editor.TextEditorTextBox.Text;
        private string VisibleText => Editor.VisibleText;
        private string? ExtraText;

        public CompletionUiList(TextEditor editor) {
            InitializeComponent();
            Editor = editor;
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

        public async void ReloadCompletionsAsync(bool selectFirst, string extraText = "") {

            ExtraText = extraText;

            if (IsBusy) {
                GotNewerRequest = true;
                return;
            }

            IsBusy = true;

            SetIntoLoadingState();

            string code = EditorText + extraText;
            string filePath = Editor.FilePath;
            (int col, int row) = Editor.GetCaretPosition();
            row++;
            col += extraText.Length;

            Script script = await Script.MakeScript(code, filePath);
            Completion[] completions = await script.Complete(row, col);

            IsBusy = false;

            if (completions.Length == 0) {
                SetIntoNoSuggestionsState();
                return;
            }

            ResetToNormalState();

            AddCompletions(completions);

            if (selectFirst)
                SelectFirst();

            if (GotNewerRequest) {
                ReloadCompletionsAsync(selectFirst, ExtraText);
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
            // This Event has to be implementd (<EventSetter Event="PreviewMouseLeftButtonDown" Handler="ListBox_MouseLeftButtonDown"/>)
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
                BorderBrush = (Brush) Application.Current.Resources["SplitterBackground"];
                Background = (Brush) Application.Current.Resources["EditorBackground"];

                WrapPanel wrapPanel = new();

                wrapPanel.Children.Add(new FontAwesome.WPF.FontAwesome() { Icon = FontAwesome.WPF.FontAwesomeIcon.Spinner, Spin = true, VerticalAlignment = VerticalAlignment.Center });
                wrapPanel.Children.Add(new TextBlock() { Text = "Loading..." });

                Child = wrapPanel;
            }
        }

        private class NoSuggestionsState : Border {
            public NoSuggestionsState() {
                BorderThickness = new(1);
                BorderBrush = (Brush) Application.Current.Resources["SplitterBackground"];
                Background = (Brush) Application.Current.Resources["EditorBackground"];
                Child = new TextBlock() { Text = "No Suggestions" };
            }
        }
    }
}
