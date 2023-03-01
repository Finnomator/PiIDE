using PiIDE.Editor.Parts;
using PiIDE.Wrapers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Completion = PiIDE.Wrapers.JediWraper.ReturnClasses.Completion;

namespace PiIDE {

    // TODO: Fix tabitems stacking. Replace with scrollbar
    // TODO: Automaticly insert indentation

    public partial class TextEditor : UserControl {

        public readonly string FilePath;
        public readonly string FileName;
        public readonly string FileExt;
        public readonly bool IsPythonFile;
        public readonly bool EnablePythonSyntaxhighlighting;
        public readonly bool EnablePylinting;
        public readonly bool EnableJediCompletions;
        public static HighlightingMode HighlightingMode => (HighlightingMode) GlobalSettings.Default.SyntaxhighlighterMode;
        public static HighlightingPerformanceMode HighlightingPerformanceMode => (HighlightingPerformanceMode) GlobalSettings.Default.SyntaxhighlighterPerformanceMode;
        public Size TextEditorTextBoxCharacterSize => MeasureTextBoxStringSize("A");

        private readonly CompletionUiList CompletionUiList;
        private int CurrentAmountOfLines;
        private (int row, int col) LastCaretPos = (1, 1);
        private readonly TextEditorCore? EditorCore;
        private readonly Canvas? EditorCoreCanvas;
        private readonly PylingUnderliner Underliner;
        protected int AutoSaveDelaySeconds;
        protected bool DoAutoSaves = true;

        public bool DisableAllWrapers { get; set; }
        public bool ContentIsSaved { get; private set; } = true;
        public bool ContentLoaded { get; private set; }

        public string EditorText => TextEditorTextBox.Text;
        public int FirstVisibleLineNum {
            get {
                int line = (int) (MainScrollViewer.VerticalOffset / TextEditorTextBoxCharacterSize.Height);
                int textLines = Tools.CountLines(EditorText);
                return textLines < line ? textLines : line;
            }
        }

        public int LastVisibleLineNum {
            get {
                int lines = (int) ((MainScrollViewer.VerticalOffset + OuterTextGrid.ActualHeight) / TextEditorTextBoxCharacterSize.Height) + 1;
                int textLines = Tools.CountLines(EditorText);
                return textLines < lines ? textLines : lines;
            }
        }
        public string VisibleText => string.Join('\n', EditorText.Split('\n')[FirstVisibleLineNum..LastVisibleLineNum]);

        public delegate void EditorFileSavedEventArgs(TextEditor sender);
        public event EditorFileSavedEventArgs? OnFileSaved;

        public event EventHandler? ContentChanged;
        public event EventHandler? StartedPythonExecution;

        public TextEditor() : this("TempFiles/temp_file1.py") {
        }

        public TextEditor(string filePath, bool disableAllWrapers = false) {
            InitializeComponent();
            FilePath = filePath;
            DisableAllWrapers = disableAllWrapers;
            FileName = Path.GetFileName(filePath);
            FileExt = Path.GetExtension(filePath);
            IsPythonFile = Tools.IsPythonExt(FileExt);
            EnablePylinting = IsPythonFile;
            EnablePythonSyntaxhighlighting = IsPythonFile;
            EnableJediCompletions = IsPythonFile;

            Loaded += delegate {
                if (!ContentLoaded)
                    ReloadFile();
                AutoSaveDelaySeconds = Tools.CountLines(EditorText) / 500 + 5;
                AutoSave();
            };

            RunFileLocalButton.IsEnabled = GlobalSettings.Default.PythonIsInstalled;
            PythonWraper.PythonExited += Python_Exited;

            // Completion suggestions stuff
            CompletionUiList = new(this);
            CompletionUiList.CompletionClicked += CompletionUiList_CompletionClick;
            TextEditorGrid.Children.Add(CompletionUiList);

            // Syntax highlighter
            if (IsPythonFile) {
                EditorCore = new(this);
                EditorCore.StartedHighlighting += delegate {
                    LoadingJediStatus.Visibility = Visibility.Visible;
                };
                EditorCore.FinishedHighlighting += delegate {
                    LoadingJediStatus.Visibility = Visibility.Collapsed;
                    if (TextEditorTextBox.Foreground is not null) {
#if DEBUG
                        TextEditorTextBox.Foreground = Brushes.Red;
#else
                        TextEditorTextBox.Foreground = null;
#endif
                    }
                };
                EditorCoreCanvas = new();
                EditorCoreCanvas.Children.Add(EditorCore);
                Grid.SetColumn(EditorCoreCanvas, 2);
                OuterTextGrid.Children.Add(EditorCoreCanvas);
            }

            // Pylint underlining stuff
            Underliner = new(this);
            TextEditorGrid.Children.Add(Underliner);

            // Searchbox stuff
            TextSearchBox.SearchChanged += TextSearchBox_SearchChanged;

            GlobalSettings.Default.PropertyChanged += delegate {
                UpdateHighlighting();
            };

            ColorResources.HighlighterColors.ColorChanged += delegate {
                UpdateHighlighting();
            };
        }

        private void TextSearchBox_SearchChanged(object? sender, Regex regex) {

            DrawingContext context = EditorCore!.OpenContext();
            Size textSize = TextEditorTextBoxCharacterSize;
            MatchCollection matches = regex.Matches(VisibleText);

            (int col, int row)[] points = Tools.GetPointsOfIndexes(VisibleText, matches.Select(x=>x.Index).ToArray());

            for (int i = 0; i < matches.Count; i++) {
                Match match = matches[i];
                int col = points[i].col;
                int row = points[i].row;

                context.DrawRectangle((Brush) Tools.BrushConverter.ConvertFromString("#50FFFFFF")!, null, new(col * textSize.Width + 2, row * 2 * textSize.Width, match.Length * textSize.Width, textSize.Height));
            }

            if (EditorCore.CurrentHighlighting is not null)
                context.DrawText(EditorCore.CurrentHighlighting, new(2, 0));

            context.Close();
        }

        private void Python_Exited(object? sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                RunFileLocalButton.IsEnabled = true;
            });
        }

        private void CompletionUiList_CompletionClick(object? sender, Completion e) => InsertCompletionAtCaret(e);

        public virtual void SaveFile(bool savedByUser) {
            try {
                File.WriteAllText(FilePath, TextEditorTextBox.Text);
                ContentIsSaved = true;
                OnFileSaved?.Invoke(this);
            } catch (Exception ex) {
                MessageBox.Show($"There was an error saving the file \"{FilePath}\"\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReloadFile() {
            try {
                TextEditorTextBox.Text = File.ReadAllText(FilePath);
                ContentLoaded = true;
            } catch (Exception ex) {
                MessageBox.Show($"There was an error loading the file \"{FilePath}\"\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReloadFile(string fileContent) => TextEditorTextBox.Text = fileContent;

        private int GetIndentOfLine(int line) {
            int index = Tools.GetIndexOfColRow(EditorText, line, 0);
            int indent = 0;
            while (indent + index < EditorText.Length && EditorText[index + indent] == ' ')
                ++indent;
            return indent;
        }

        private char? LastTypedChar() {
            int caret = TextEditorTextBox.CaretIndex;
            if (caret <= 0)
                return null;
            return EditorText[caret - 1];
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
                        InsertCompletionAtCaret(CompletionUiList.SelectedCompletion!);
                        CompletionUiList.Close();
                        e.Handled = true;
                    } else {
                        char? charInFrontOfCaret = LastTypedChar();

                        if (charInFrontOfCaret is null)
                            break;

                        int caretRow = GetCaretRow();

                        int indentAmount = GetIndentOfLine(caretRow);

                        InsertAtCaretAndMoveCaret("\r\n" + new string(' ', indentAmount + (charInFrontOfCaret == ':' ? 4 : 0)));

                        e.Handled = true;
                    }
                    break;
                case Key.Tab:
                    HandleTabKey();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    if (CompletionUiList.IsOpen) {
                        CompletionUiList.Close();
                        e.Handled = true;
                    }
                    break;
            }

            foreach (Shortcut shortcut in Shortcuts.ShortcutsMap.Keys) {
                List<Key> hotkey = Shortcuts.ShortcutsMap[shortcut];

                // because we dont want to save twice when we press ctrl + s (just as example)
                if (!Shortcuts.IsShortcutPressed(shortcut) || hotkey[^1] != e.Key)
                    continue;

                switch (shortcut) {
                    case Shortcut.SaveFile:
                        SaveFile(true);
                        break;
                    case Shortcut.OpenCompletionsList:
                        DisplayCodeCompletionsAsync();
                        break;
                }
                e.Handled = true;
            }
        }

        private (int firstSelectedLine, int lastSelectedLine) GetSelectedLines() {
            int selectionStart = TextEditorTextBox.SelectionStart;
            int selectionLength = TextEditorTextBox.SelectionLength;
            (int col, int row)[] lines = Tools.GetPointsOfIndexes(EditorText, new int[] { selectionStart, selectionStart + selectionLength });
            return (lines.Min().row, lines.Max().row);
        }

        private void IndentLines(int[] lines) {
            int oldStart = TextEditorTextBox.SelectionStart;
            int oldLength = TextEditorTextBox.SelectionLength;
            string newText = EditorText;
            foreach (int line in lines) {
                int indent = 4 - GetIndentOfLine(line) % 4;
                if (indent == 0)
                    indent = 4;
                newText = newText.Insert(newText.GetIndexOfColRow(line, 0), new string(' ', indent));

                if (line == lines[0])
                    oldStart += indent;
                else
                    oldLength += indent;
            }
            TextEditorTextBox.Text = newText;
            TextEditorTextBox.SelectionStart = oldStart;
            TextEditorTextBox.SelectionLength = oldLength;
        }

        private void OutdentLines(int[] lines) {
            int oldStart = TextEditorTextBox.SelectionStart;
            int oldLength = TextEditorTextBox.SelectionLength;
            string newText = EditorText;

            foreach (int line in lines) {
                int indent = GetIndentOfLine(line);

                if (indent == 0)
                    continue;

                int goBack = indent % 4;

                if (goBack == 0)
                    goBack = 4;

                int index = newText.GetIndexOfColRow(line, 0);
                newText = newText[..index] + newText[(index + goBack)..];

                if (line == lines[0] && oldStart >= goBack)
                    oldStart -= goBack;
                else
                    oldLength -= goBack;
            }
            TextEditorTextBox.Text = newText;
            TextEditorTextBox.SelectionStart = oldStart;
            TextEditorTextBox.SelectionLength = oldLength;
        }

        private void HandleTabKey() {

            int selectionLength = TextEditorTextBox.SelectionLength;

            if (selectionLength != 0) {
                (int firstSelectedLine, int lastSelectedLine) = GetSelectedLines();
                int lineDelta = lastSelectedLine - firstSelectedLine;

                if (lineDelta == 0) {
                    string text = TextEditorTextBox.Text;
                    int index = TextEditorTextBox.CaretIndex;
                    TextEditorTextBox.Text = $"{text[..index]}    {text[(index + selectionLength)..]}";
                    TextEditorTextBox.CaretIndex = index + 4;
                } else {
                    int[] lineNums = new int[lineDelta + 1];
                    for (int i = 0; i <= lineDelta; i++)
                        lineNums[i] = firstSelectedLine + i;
                    if (Keyboard.IsKeyDown(Key.LeftShift))
                        OutdentLines(lineNums);
                    else
                        IndentLines(lineNums);
                }
                return;
            }

            if (CompletionUiList.SelectedAnIndex) {
                InsertCompletionAtCaret(CompletionUiList.SelectedCompletion!);
                CompletionUiList.Close();
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftShift)) {
                int col = GetCaretPosition().col;
                int goBack = col % 4;
                if (col > 0)
                    TextEditorTextBox.CaretIndex -= goBack == 0 ? 4 : goBack;
                return;
            }

            // TODO: replace with a variable option
            int spaceToFillIndent = 4 - GetCaretPosition().col % 4;
            if (spaceToFillIndent == 0)
                spaceToFillIndent = 4;
            InsertAtCaretAndMoveCaret(new string(' ', spaceToFillIndent));
        }

        private void InsertCompletionAtCaret(Completion completion) {

            if (completion.Complete is null || completion.Complete.Length == 0)
                return;

            int oldCaretIndex = TextEditorTextBox.CaretIndex;
            int completionStart = oldCaretIndex - (completion.Name.Length - completion.Complete.Length);
            int toRemoveLen = completion.Name.Length - completion.Complete.Length;

            TextEditorTextBox.Text = TextEditorTextBox.Text.Remove(completionStart, toRemoveLen);
            TextEditorTextBox.Text = TextEditorTextBox.Text.Insert(completionStart, completion.Name);

            int newIndex = oldCaretIndex + completion.Complete.Length;

            TextEditorTextBox.CaretIndex = newIndex < 0 ? 0 : newIndex;
        }

        private void InsertAtCaretAndMoveCaret(string text) {
            int oldCaretIndex = TextEditorTextBox.CaretIndex;
            TextEditorTextBox.Text = TextEditorTextBox.Text.Insert(oldCaretIndex, text);
            TextEditorTextBox.CaretIndex = oldCaretIndex + text.Length;
        }

        public static string GetLineNumbers(int lines) => string.Join(Environment.NewLine, Enumerable.Range(1, lines));

        private void DisplayCodeCompletionsAsync() {

            if (!EnableJediCompletions || !GlobalSettings.Default.JediIsUsable)
                return;

            Thickness marginAtCaretPos = MarginAtCaretPosition();
            CompletionUiList.Margin = marginAtCaretPos;

            CompletionUiList.ReloadCompletionsAsync(true);
        }

        protected virtual void TextEditorTextBox_TextChanged(object sender, TextChangedEventArgs e) {

            ContentIsSaved = !ContentLoaded;
            ContentChanged?.Invoke(this, e);
            UpdateHighlighting();

            char? lastChar = LastTypedChar();

            if (lastChar is not null && char.IsLetter((char) lastChar) || lastChar == '.' || lastChar == '_')
                DisplayCodeCompletionsAsync();
            else
                CompletionUiList.Close();

            int textLines = Tools.CountLines(TextEditorTextBox.Text);

            if (textLines != CurrentAmountOfLines) {
                NumsTextBlock.Text = GetLineNumbers(textLines);
                CurrentAmountOfLines = textLines;
            }
        }

        private Thickness MarginAtCaretPosition() {
            (int col, int row) = GetCaretPosition();
            return new(
                (col + 0.5) * TextEditorTextBoxCharacterSize.Width,
                (row + 1) * TextEditorTextBoxCharacterSize.Height,
                0,
                0
            );
        }

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
            return new(formattedText.Width, formattedText.Height);
        }

        private async void AutoSave() {
            while (DoAutoSaves) {
                await Task.Delay(AutoSaveDelaySeconds * 1000);
                if (!ContentIsSaved)
                    SaveFile(false);
            }
        }

        public int GetCaretRow() => Tools.GetRowOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex);
        public (int col, int row) GetCaretPosition() => Tools.GetPointOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex);

        private void UpdateHighlighting() {

            if (DisableAllWrapers || EditorCore is null)
                return;

            double remainder = MainScrollViewer.VerticalOffset % TextEditorTextBoxCharacterSize.Height;

            EditorCoreCanvas!.Margin = new(-MainScrollViewer.HorizontalOffset, Math.Abs(remainder - TextEditorTextBoxCharacterSize.Height) < 0.1 ? 0 : -remainder, 0, 0);
            EditorCore.UpdateTextAsync(HighlightingMode, HighlightingPerformanceMode);
        }

        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {

            if (e.HorizontalChange == 0 && e.VerticalChange == 0)
                return;

            UpdateHighlighting();
            UpdatePylint();
        }

        public void SetCaretPosition(int line, int column) => TextEditorTextBox.CaretIndex = Tools.GetIndexOfColRow(TextEditorTextBox.Text, line, column);

        public void ScrollToCaret() {
            double verticalOffset = Tools.GetRowOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex) * TextEditorTextBoxCharacterSize.Height;
            double horizontalOffset = Tools.GetColOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex) * TextEditorTextBoxCharacterSize.Width;

            MainScrollViewer.ScrollToVerticalOffset(verticalOffset - (ActualHeight / 2));
            MainScrollViewer.ScrollToHorizontalOffset(horizontalOffset - (ActualHeight / 2));
        }

        public void ScrollToPosition(int line, int column) {
            double verticalOffset = line * TextEditorTextBoxCharacterSize.Height;
            double horizontalOffset = column * TextEditorTextBoxCharacterSize.Width;

            MainScrollViewer.ScrollToVerticalOffset(verticalOffset - (ActualHeight / 2));
            MainScrollViewer.ScrollToHorizontalOffset(horizontalOffset - (ActualHeight / 2));
        }

        private void TextEditorTextBox_LostFocus(object sender, RoutedEventArgs e) {
            CompletionUiList.Close();
        }

        private void TextEditorTextBox_MouseDown(object sender, MouseButtonEventArgs e) => CompletionUiList.Close();

        private void RunFileLocalButton_Click(object sender, RoutedEventArgs e) {
            RunFileLocalButton.IsEnabled = false;
            SaveFile(false);
            PythonWraper.AsyncFileRunner.RunFileAsync(FilePath);
            StartedPythonExecution?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void StopAllRunningTasksButton_Click(object sender, RoutedEventArgs e) {
            PythonWraper.AsyncFileRunner.KillProcess();
            RunFileLocalButton.IsEnabled = GlobalSettings.Default.PythonIsInstalled;
        }

        public void UpdatePylint(PylintMessage[] pylintMessages) {
            if (EnablePylinting && GlobalSettings.Default.PylintIsUsable) {
                Underliner.Underline(pylintMessages, FirstVisibleLineNum, LastVisibleLineNum);
                AmountOfErrorsLabel.Content = pylintMessages.Where(x => x.Type == "error").Count();
                AmountOfWarningsLabel.Content = pylintMessages.Where(x => x.Type == "warning").Count();
            }
        }

        public void UpdatePylint() {
            if (EnablePylinting && GlobalSettings.Default.PylintIsUsable)
                Underliner.UpdateUnderline(FirstVisibleLineNum, LastVisibleLineNum);
        }

        private void MainScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
            int deltaLines = -e.Delta / 60;
            if (deltaLines == 0 && e.Delta != 0)
                deltaLines = e.Delta < 0 ? 1 : (-1);
            MainScrollViewer.ScrollToVerticalOffset((FirstVisibleLineNum + deltaLines) * TextEditorTextBoxCharacterSize.Height);
        }

        private void TextEditorTextBox_SelectionChanged(object sender, RoutedEventArgs e) {
            (int col, int row) = GetCaretPosition();
            if (LastCaretPos.row == row && LastCaretPos.col == col)
                return;
            LastCaretPos.row = row;
            LastCaretPos.col = col;

            CaretColTextBlock.Text = (col + 1).ToString();
            CaretRowTextBlock.Text = (row + 1).ToString();
        }

        private void MainScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) {
            UpdateHighlighting();
        }

        private void Rectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                TextEditorTextBox.Focus();
                TextEditorTextBox.CaretIndex = EditorText.Length;
                e.Handled = true;
            }
        }
    }
}
