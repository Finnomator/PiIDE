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
        public readonly bool IsBoardFile;

        public bool DisableAllWrapers = true;

        private bool BlockCompletions = true;
        private readonly CompletionUiList CompletionUiList;
        private string OldTextEditorTextBoxText;
        private int CurrentAmountOfLines;
        private Size TextEditorTextBoxCharacterSize;
        private readonly SyntaxHighlighter Highlighter;
        private readonly BoardFileViewItem? BoardViewItem;
        public readonly PylingUnderliner Underliner;

        public int FirstVisibleLineNum { get; private set; }
        public int LastVisibleLineNum { get; private set; }

        public event EventHandler<string>? OnFileSaved;

        public TextEditor() : this("TempFiles/temp_file1.py") {
        }

        public TextEditor(string filePath, BoardFileViewItem? boardViewItem = null) {
            InitializeComponent();
            FilePath = filePath;
            IsBoardFile = boardViewItem is not null;
            BoardViewItem = boardViewItem;
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
            TextEditorGrid.Children.Add(Highlighter);

            Underliner = new(TextEditorTextBoxCharacterSize);
            TextEditorGrid.Children.Add(Underliner);

            if (EnablePythonSyntaxhighlighting)
                UpdateHighlighting();

            BlockCompletions = false;
        }

        private void CompletionUiList_CompletionClick(object? sender, Completion e) {
            InsertCompletionAtCaret(e);
        }

        public void SaveFile() {
            File.WriteAllText(FilePath, TextEditorTextBox.Text);
            if (IsBoardFile)
                AmpyWraper.WriteToBoard(BoardViewItem.COMPort, FilePath, BoardViewItem.BoardFilePath);
            OnFileSaved?.Invoke(this, FilePath);
        }

        public void ReloadFile() {
            string fileContent = File.ReadAllText(FilePath);
            string[] fileLines = fileContent.Split("\r\n");
            OldTextEditorTextBoxText = fileContent;
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
                    } else // TODO: replace with a variable option
                        InsertAtCaretAndMoveCaret("    ");
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

            if (BlockCompletions || !EnableJediCompletions)
                return;

            await CompletionUiList.ReloadCompletionsAsync(TextEditorTextBox.Text, GetCaretRow() + 1, GetCaretCol());

            CompletionUiList.Margin = MarginAtCaretPosition();
        }

        private async void TextEditorTextBox_TextChangedAsync(object sender, TextChangedEventArgs e) {

            // TODO: Improve Opening of Completions because it often gets in the way

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

        private int GetCaretCol() => Tools.GetColOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex);
        private int GetCaretRow() => Tools.GetRowOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex);

        private async void UpdateHighlighting() {

            while (true) {

                while (DisableAllWrapers)
                    await Task.Delay(1000);

                Highlighter.HighglightText(TextEditorTextBox.Text, FilePath, FirstVisibleLineNum, LastVisibleLineNum);

                await Task.Delay((LastVisibleLineNum - FirstVisibleLineNum) * 10 + 50);
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
    }
}
