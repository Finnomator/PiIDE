using PiIDE.Wrapers;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PiIDE {

    // TODO: Fix tabitems stacking. Replace with scrollbar
    // TODO: Add "Saved on Board/Local" identifier
    // TODO: Automaticly insert indentation

    public partial class TextEditor : UserControl {

        public readonly string FilePath;
        public readonly string FileName;
        public readonly string FileExt;

        public readonly bool IsPythonFile;
        public readonly bool EnablePythonSyntaxhighlighting;
        public readonly bool EnablePylinging;
        public readonly bool EnableJediCompletions;

        public bool DisableAllWrapers = true;

        private bool BlockCompletions = true;
        private readonly CompletionUiList CompletionUiList;
        private int CurrentAmountOfLines;
        private Size TextEditorTextBoxCharacterSize;
        private readonly SyntaxHighlighter Highlighter;
        public readonly PylingUnderliner Underliner;

        public int FirstVisibleLineNum { get; private set; }
        public int LastVisibleLineNum { get; private set; }

        public event EventHandler<string>? OnFileSaved;

        public TextEditor() : this("TempFiles/temp_file1.py") {
        }

        public TextEditor(string filePath) {
            InitializeComponent();
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            FileExt = Path.GetExtension(filePath);

            IsPythonFile = Tools.IsPythonExt(FileExt);
            EnablePylinging = IsPythonFile;
            EnablePythonSyntaxhighlighting = IsPythonFile;
            EnableJediCompletions = IsPythonFile;

            CompletionUiList = new(FilePath);
            CompletionUiList.CompletionClicked += CompletionUiList_CompletionClick;
            Panel.SetZIndex(CompletionUiList, 1);
            TextEditorGrid.Children.Add(CompletionUiList);

            ReloadFile();

            TextEditorTextBoxCharacterSize = MeasureTextBoxStringSize("A");

            Highlighter = new(TextEditorTextBoxCharacterSize);
            Highlighter.OnHoverOverWord += Highlighter_OnHoverOverWord;
            TextEditorGrid.Children.Add(Highlighter);

            Underliner = new(TextEditorTextBoxCharacterSize);
            TextEditorGrid.Children.Add(Underliner);

            if (EnablePythonSyntaxhighlighting && GlobalSettings.Default.JediIsUsable)
                UpdateHighlighting();

            BlockCompletions = false;
        }

        private void Highlighter_OnHoverOverWord(object? sender, string e) {
            Debug.WriteLine("Hover");
        }

        private void CompletionUiList_CompletionClick(object? sender, Completion e) => InsertCompletionAtCaret(e);

        public virtual void SaveFile() {
            File.WriteAllText(FilePath, TextEditorTextBox.Text);
            OnFileSaved?.Invoke(this, FilePath);
        }

        public void ReloadFile() {
            string fileContent = File.ReadAllText(FilePath);
            string[] fileLines = fileContent.Split("\r\n");
            TextEditorTextBox.Text = fileContent;
            NumsTextBlock.Text = GetLineNumbers(fileLines.Length);
        }

        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e) {

            switch (e.Key) {

                case Key.Down:
                    if (CompletionUiList.IsOpen) {
                        CompletionUiList.MoveSelectedCompletionDown();
                        e.Handled = true;
                    }
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
                    } else {
                        // TODO: replace with a variable option
                        int spaceToFillIndent = 4 - GetCaretCol() % 4;
                        if (spaceToFillIndent == 0)
                            spaceToFillIndent = 4;
                        InsertAtCaretAndMoveCaret(new string(' ', spaceToFillIndent));
                    }

                    e.Handled = true;
                    break;
                case Key.S:
                    if (Shortcuts.IsShortcutPressed(Shortcut.SaveFile)) {
                        SaveFile();
                        e.Handled = true;
                    }
                    break;
                case Key.Space:
                    if (Shortcuts.IsShortcutPressed(Shortcut.OpenCompletionsList)) {
                        DisplayCodeCompletionsAsync();
                        e.Handled = true;
                    }
                    break;
                case Key.Escape:
                    if (CompletionUiList.IsOpen) {
                        CompletionUiList.Close();
                        e.Handled = true;
                    }
                    break;
            }

            if (e.Handled)
                return;

            string keyString = e.Key.ToString();

            //TODO: dont open when key combos (like ctrl + a) are pressed

            if (keyString.Length != 1) {
                CompletionUiList.Close();
                return;
            }

            char keyChar = keyString[0];
            if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift)) {
                keyChar = char.ToLower(keyChar);
                keyString = keyString.ToLower();
            }

            if (!char.IsLetter(keyChar) || !JediCompletionWraper.FinishedGettingCompletions) {
                CompletionUiList.Close();
                return;
            }

            DisplayCodeCompletionsWithExtraTextAsync(keyString);
        }

        private void InsertCompletionAtCaret(Completion completion) {

            // TODO: Fix that sometimes the completion is outdated

            if (completion.Complete.Length == 0)
                return;

            BlockCompletions = true;

            int oldCaretIndex = TextEditorTextBox.CaretIndex;
            int completionStart = oldCaretIndex - (completion.Name.Length - completion.Complete.Length);
            int toRemoveLen = completion.Name.Length - completion.Complete.Length;

            TextEditorTextBox.Text = TextEditorTextBox.Text.Remove(completionStart, toRemoveLen);
            TextEditorTextBox.Text = TextEditorTextBox.Text.Insert(completionStart, completion.Name);

            int newIndex = oldCaretIndex + completion.Complete.Length;

            TextEditorTextBox.CaretIndex = newIndex < 0 ? 0 : newIndex;

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

        private void DisplayCodeCompletionsAsync() => DisplayCodeCompletionsWithExtraTextAsync("");

        private async void DisplayCodeCompletionsWithExtraTextAsync(string extraText) {

            // This method is intended to be used when the user presses a key and the input is not in textbox yet

            if (BlockCompletions || !EnableJediCompletions || !GlobalSettings.Default.JediIsUsable)
                return;

            await CompletionUiList.ReloadCompletionsAsync(TextEditorTextBox.Text.Insert(TextEditorTextBox.CaretIndex, extraText), GetCaretRow() + 1, GetCaretCol() + extraText.Length);
            CompletionUiList.SelectFirst();
            CompletionUiList.Margin = MarginAtCaretPosition();
        }

        private void TextEditorTextBox_TextChanged(object sender, TextChangedEventArgs e) {

            string[] textLines = TextEditorTextBox.Text.Split("\r\n");

            if (textLines.Length != CurrentAmountOfLines) {
                NumsTextBlock.Text = GetLineNumbers(textLines.Length);
                CurrentAmountOfLines = textLines.Length;
            }
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

        private int GetCaretCol() => Tools.GetColOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex);
        private int GetCaretRow() => Tools.GetRowOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex);

        private async void UpdateHighlighting() {

            while (true) {

                while (DisableAllWrapers)
                    await Task.Delay(1000);

                Highlighter.HighglightText(TextEditorTextBox.Text, FilePath, FirstVisibleLineNum, LastVisibleLineNum);

                await Task.Delay((LastVisibleLineNum - FirstVisibleLineNum) * 5 + 50);
            }
        }

        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            FirstVisibleLineNum = (int) (e.VerticalOffset / TextEditorTextBoxCharacterSize.Height);
            LastVisibleLineNum = (int) ((e.VerticalOffset + TextEditorTextBox.ActualHeight) / TextEditorTextBoxCharacterSize.Height);
        }

        public void SetCaretPositioin(int line, int column) => TextEditorTextBox.CaretIndex = Tools.GetIndexOfColRow(TextEditorTextBox.Text, line, column);
        public void ScrollToCaret() {
            // TODO: Implement something to calculate col and row
            double verticalOffset = Tools.GetRowOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex) * TextEditorTextBoxCharacterSize.Height;
            double horizontalOffset = Tools.GetColOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex) * TextEditorTextBoxCharacterSize.Width;

            MainScrollViewer.ScrollToVerticalOffset(verticalOffset - (ActualHeight * 0.5));
            MainScrollViewer.ScrollToHorizontalOffset(horizontalOffset - (ActualHeight * 0.5));
        }
        public void ScrollToPosition(int line, int column) {
            double verticalOffset = line * TextEditorTextBoxCharacterSize.Height;
            double horizontalOffset = column * TextEditorTextBoxCharacterSize.Width;

            MainScrollViewer.ScrollToVerticalOffset(verticalOffset - (ActualHeight * 0.5));
            MainScrollViewer.ScrollToHorizontalOffset(horizontalOffset - (ActualHeight * 0.5));
        }

        private void TextEditorTextBox_LostFocus(object sender, RoutedEventArgs e) => CompletionUiList.Close();

        private void TextEditorTextBox_MouseDown(object sender, MouseButtonEventArgs e) => CompletionUiList.Close();
    }
}
