using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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

        public List<string> GetLineNumbers(int lines) {
            List<string> items = new();
            for (int i = 1; i <= lines; ++i)
                items.Add(i.ToString());
            return items;
        }

        public int GetCaretLine() {
            string[] lines = TextEditorTextBox.Text.Split("\n\r");
            int totalLen = lines[0].Length;
            int line = 0;
            for (; TextEditorTextBox.CaretIndex > totalLen; line++) {
                totalLen += lines[line].Length;
            }
            return line + 1;
        }

        public int CaretColumn => TextEditorTextBox.CaretIndex - TextEditorTextBox.GetCharacterIndexFromLineIndex(GetCaretLine() - 1) + 1;

        private void TextEditorTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            File.WriteAllText(FilePath, TextEditorTextBox.Text);
            Debug.WriteLine(JediCompletionWraper.GetCompletion(FilePath, GetCaretLine(), CaretColumn));
        }
    }
}
