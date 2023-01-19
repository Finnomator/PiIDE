using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PiIDE {

    public partial class TextEditor : UserControl {

        private string _FilePath = "";
        public string FilePath {
            set {
                if (string.IsNullOrEmpty(_FilePath))
                    _FilePath = value;
            }
            get { return _FilePath; }
        }
        private bool BlockCompletions = true;

        public TextEditor() {
            InitializeComponent();
            LineNumsListBox.ItemsSource = GetLineNumbers(1);
            BlockCompletions = false;
        }

        public TextEditor(string filePath) {
            FilePath = filePath;
            InitializeComponent();
            string[] fileLines = File.ReadAllLines(filePath);
            string fileContent = File.ReadAllText(filePath);
            TextEditorTextBox.Text = fileContent;
            LineNumsListBox.ItemsSource = GetLineNumbers(fileLines.Length);
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
                        InsertAtCaretAndMoveCaret(CompletionUiList.SelectedCompletion.Complete);
                        e.Handled = true;
                    }

                    break;

                case Key.Tab:

                    if (CompletionUiList.SelectedAnIndex)
                        InsertAtCaretAndMoveCaret(CompletionUiList.SelectedCompletion.Complete);
                    else
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
            }
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

            //Completion[] completions = JediCompletionWraper.GetCompletion(FilePath, GetCaretRow() + 1, GetCaretCol());
            var completions = new Completion[0];
            if (completions.Length == 0)
                return;

            CompletionUiList.ClearCompletions();
            CompletionUiList.AddCompletions(completions);
            CompletionUiList.Margin = MarginAtCaretPosition();
        }

        private void TextEditorTextBox_TextChanged(object sender, TextChangedEventArgs e) {

            CompletionUiList.ClearCompletions();

            File.WriteAllText(FilePath, TextEditorTextBox.Text);

            DisplayCodeCompletions();
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
    }
}
