using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PiIDE {

    public partial class TextEditor : UserControl {

        public readonly string FilePath;
        private bool BlockCompletions = true;
        private readonly CompletionUiList CompletionUiList;
        private string OldTextEditorTextBoxText;
        private int CurrentAmountOfLines;
        private Size TextEditorTextBoxCharacterSize;
        private readonly SyntaxHighlighter Highlighter;

        public TextEditor(string filePath) {

            InitializeComponent();
            FilePath = filePath;

            CompletionUiList = new(FilePath);

            CompletionsContainerCanvas.Children.Add(CompletionUiList);

            string[] fileLines = File.ReadAllLines(FilePath);
            string fileContent = File.ReadAllText(FilePath);
            OldTextEditorTextBoxText = fileContent;
            TextEditorTextBox.Text = fileContent;

            NumsTextBlock.Text = GetLineNumbers(fileLines.Length);
            TextEditorTextBoxCharacterSize = MeasureTextBoxStringSize("A");

            Highlighter = new(TextEditorTextBoxCharacterSize);
            TextEditorGrid.Children.Add(Highlighter);

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
                        DisplayCodeCompletionsAsync();
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

        public static string GetLineNumbers(int lines) => string.Join(Environment.NewLine, Enumerable.Range(1, lines));

        private async Task DisplayCodeCompletionsAsync() {

            if (BlockCompletions)
                return;

            await CompletionUiList.ReloadCompletionsAsync(GetCaretRow() + 1, GetCaretCol());

            CompletionUiList.Margin = MarginAtCaretPosition();
        }

        private async void TextEditorTextBox_TextChangedAsync(object sender, TextChangedEventArgs e) {

            if (FilePath == "" || OldTextEditorTextBoxText == "") {
                OldTextEditorTextBoxText = TextEditorTextBox.Text;
                return;
            }

            string textDifference = TextEditorTextBox.Text.Replace(OldTextEditorTextBoxText, "");

            string[] textLines = TextEditorTextBox.Text.Split("\r\n");

            if (textLines.Length != CurrentAmountOfLines) {
                NumsTextBlock.Text = GetLineNumbers(textLines.Length);
                CurrentAmountOfLines = textLines.Length;
            }

            if (!string.IsNullOrWhiteSpace(textDifference) && JediCompletionWraper.FinishedGettingCompletions) {

                File.WriteAllText(FilePath, TextEditorTextBox.Text);

                await DisplayCodeCompletionsAsync();
            }

            OldTextEditorTextBoxText = TextEditorTextBox.Text;
        }

        private Thickness MarginAtCaretPosition() => new((GetCaretCol() + 0.5) * TextEditorTextBoxCharacterSize.Width, (GetCaretRow() + 1) * TextEditorTextBoxCharacterSize.Height, 0, 0);

        private Size MeasureTextBoxStringSize(string candidate) {
            FormattedText formattedText = new(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(TextEditorTextBox.FontFamily, TextEditorTextBox.FontStyle, TextEditorTextBox.FontWeight, TextEditorTextBox.FontStretch),
                TextEditorTextBox.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                VisualTreeHelper.GetDpi(TextEditorTextBox).PixelsPerDip);
            return new Size(formattedText.Width, formattedText.Height);
        }

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

                await Highlighter.HighglightTextAsync(TextEditorTextBox.Text, FilePath);

                await Task.Delay(50);

            }
        }
    }
}
