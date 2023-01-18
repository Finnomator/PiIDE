using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
            
        }

        public static List<string> GetLineNumbers(int lines) {
            List<string> items = new();
            for (int i = 1; i <= lines; ++i)
                items.Add(i.ToString());
            return items;
        }

        private void TextEditorTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            File.WriteAllText(FilePath, TextEditorTextBox.Text);

            string text = TextEditorTextBox.Text;

            Dictionary<string, Completion> completions = JediCompletionWraper.GetCompletion(FilePath, GetCaretRow() + 1, GetCaretCol());

            CompletionUiList.ClearCompletions();
            CompletionUiList.AddCompletions(completions.Values.ToArray());
            CompletionUiList.Margin = MarginAtCaretPosition();
        }

        private int GetCaretRow() {
            throw new NotImplementedException();
        }

        private Thickness MarginAtCaretPosition() => new((GetCaretCol() + 0.5) * 7.0, GetCaretRow() * 16.0, 0, 0);
    }
}
