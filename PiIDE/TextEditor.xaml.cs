using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PiIDE {

    public partial class TextEditor : UserControl {

        public readonly string FilePath;
        private bool BlockCompletions = true;
        private readonly CompletionUiList CompletionUiList;
        private string OldTextEditorTextBoxText;

        public TextEditor(string filePath) {

            InitializeComponent();
            FilePath = filePath;

            CompletionUiList = new(FilePath);

            CompletionsContainerCanvas.Children.Add(CompletionUiList);

            string[] fileLines = File.ReadAllLines(FilePath);
            string fileContent = File.ReadAllText(FilePath);
            OldTextEditorTextBoxText = fileContent;
            TextEditorTextBox.Text = fileContent;

            LineNumsListBox.ItemsSource = GetLineNumbers(fileLines.Length);

            UpdatePygmentize();

            BlockCompletions = false;
        }

        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e) {

            switch (e.Key) {

                case Key.Down:
                    CompletionUiList.MoveSelectedCompletionDown();
                    e.Handled = true;
                    break;
                case Key.Up:
                    if (CompletionUiList.SelectedAnIndex) {
                        CompletionUiList.MoveSelectedCompletionUp();
                        e.Handled = true;
                    }
                    break;

                case Key.Enter:

                    if (CompletionUiList.SelectedAnIndex) {
                        InsertCompletionAtCaret(CompletionUiList.SelectedCompletion);
                        CompletionUiList.Close();
                        e.Handled = true;
                    }

                    break;

                case Key.Tab:

                    if (CompletionUiList.SelectedAnIndex) {
                        InsertCompletionAtCaret(CompletionUiList.SelectedCompletion);
                        CompletionUiList.Close();
                    } else
                        InsertAtCaretAndMoveCaret("    ");

                    e.Handled = true;
                    break;

                case Key.LeftCtrl:

                    if (Tools.IsShortCutPressed(Key.LeftCtrl, Key.Space)) {
                        DisplayCodeCompletions();
                        e.Handled = true;
                    }

                    break;

                case Key.Space:

                    if (Tools.IsShortCutPressed(Key.LeftCtrl, Key.Space)) {
                        e.Handled = true;
                    }

                    break;

                case Key.Escape:
                    if (CompletionUiList.SelectedAnIndex) {
                        CompletionUiList.Close();
                        e.Handled = true;
                    }

                    break;
            }
        }

        private void InsertCompletionAtCaret(Completion completion) {
            BlockCompletions = true;
            int oldCaretIndex = TextEditorTextBox.CaretIndex;
            int completionStart = oldCaretIndex - (completion.Name.Length - completion.Complete.Length);
            TextEditorTextBox.Text = TextEditorTextBox.Text.Remove(completionStart, completion.Name.Length - completion.Complete.Length);
            TextEditorTextBox.Text = TextEditorTextBox.Text.Insert(completionStart, completion.Name);
            TextEditorTextBox.CaretIndex = oldCaretIndex + completion.Complete.Length;
            BlockCompletions = false;
        }

        private void InsertAtCaretAndMoveCaret(string text) {
            BlockCompletions = true;
            int oldCaretIndex = TextEditorTextBox.CaretIndex;
            TextEditorTextBox.Text = TextEditorTextBox.Text.Insert(oldCaretIndex, text);
            TextEditorTextBox.CaretIndex = oldCaretIndex + text.Length;
            BlockCompletions = false;
        }

        public static List<string> GetLineNumbers(int lines) {
            List<string> items = new();
            for (int i = 1; i <= lines; ++i)
                items.Add(i.ToString());
            return items;
        }

        private void DisplayCodeCompletions() {

            if (BlockCompletions)
                return;

            CompletionUiList.ReloadCompletions(GetCaretRow() + 1, GetCaretCol());

            CompletionUiList.Margin = MarginAtCaretPosition();
        }

        private void TextEditorTextBox_TextChanged(object sender, TextChangedEventArgs e) {

            if (FilePath == "" || OldTextEditorTextBoxText == "") {
                OldTextEditorTextBoxText = TextEditorTextBox.Text;
                return;
            }

            string textDifference = TextEditorTextBox.Text.Replace(OldTextEditorTextBoxText, "");

            if (!string.IsNullOrWhiteSpace(textDifference)) {

                File.WriteAllText(FilePath, TextEditorTextBox.Text);

                DisplayCodeCompletions();

            }

            OldTextEditorTextBoxText = TextEditorTextBox.Text;
        }

        private Thickness MarginAtCaretPosition() => new((GetCaretCol() + 0.5) * 7.0, (GetCaretRow() + 1) * 14.0, 0, 0);

        private int GetCaretCol() {
            int caretLine = GetCaretRow();

            int offset = 0;
            string[] lines = TextEditorTextBox.Text.Split("\r\n");

            for (int i = 0; i < caretLine; ++i)
                offset += lines[i].Length + 2;

            return TextEditorTextBox.CaretIndex - offset;
        }

        private int GetCaretRow() {

            int offset = 0;
            string[] lines = TextEditorTextBox.Text.Split("\r\n");
            int line = 0;

            for (; TextEditorTextBox.CaretIndex >= offset; ++line)
                offset += lines[line].Length + 2;

            return line - 1;
        }

        private async void UpdatePygmentize() {

            while (true) {

                await Task.Delay(500);

                string newHtml = PygmentizerWraper.GetPygmentizedString(FilePath);

                if (string.IsNullOrEmpty(newHtml))
                    continue;

                HTMLContentPresenter.NavigateToString(newHtml);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Window window = Window.GetWindow(HTMLContentPresenter);
            window.LocationChanged += delegate (object sender, EventArgs e) {
                double offset = OverlayingPopup.HorizontalOffset;
                OverlayingPopup.HorizontalOffset = offset + 1;
                OverlayingPopup.HorizontalOffset = offset;
            };
        }
    }
}
