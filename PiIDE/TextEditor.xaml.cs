using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PiIDE {

    public partial class TextEditor : UserControl {

        public readonly string FilePath;

        public TextEditor() {
            FilePath = "";
            InitializeComponent();
            LineNumsListBox.ItemsSource = GetLineNumbers(1);
        }

        public TextEditor(string filePath) {
            FilePath = filePath;
            InitializeComponent();
            string[] fileLines = File.ReadAllLines(filePath);
            string fileContent = File.ReadAllText(filePath);
            TextEditorTextBox.Text = fileContent;
            LineNumsListBox.ItemsSource = GetLineNumbers(fileLines.Length);
        }

        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Down:
                    CompletionUiList.MoveSelectedCompletionDown();
                    e.Handled = true;
                    break;
                case Key.Up:
                    CompletionUiList.MoveSelectedCompletionUp();
                    e.Handled = true;
                    break;

                case Key.Enter:

                    if (CompletionUiList.SelectedAnIndex) {
                        TextEditorTextBox.Text.Insert(TextEditorTextBox.CaretIndex, CompletionUiList.SelectedCompletion.Complete);
                    }

                    break;
            }
        }

        public static List<string> GetLineNumbers(int lines) {
            List<string> items = new();
            for (int i = 1; i <= lines; ++i)
                items.Add(i.ToString());
            return items;
        }

        private void TextEditorTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            File.WriteAllText(FilePath, TextEditorTextBox.Text);

            Dictionary<string, Completion> completions = JediCompletionWraper.GetCompletion(FilePath, GetCaretRow() + 1, GetCaretCol());

            CompletionUiList.ClearCompletions();
            CompletionUiList.AddCompletions(completions.Values.ToArray());
            CompletionUiList.Margin = MarginAtCaretPosition();
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
