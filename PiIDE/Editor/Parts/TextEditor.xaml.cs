using PiIDE.Editor.Parts;
using PiIDE.Wrapers;
using System;
using System.Diagnostics;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Point = System.Drawing.Point;
using Completion = PiIDE.Wrapers.JediWraper.ReturnClasses.Completion;
using JediName = PiIDE.Wrapers.JediWraper.ReturnClasses.Name;
using static PiIDE.Wrapers.JediWraper;

namespace PiIDE {

    // TODO: Fix tabitems stacking. Replace with scrollbar
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
        public bool ContentIsSaved { get; set; } = true;

        public bool DisableAllWrapers = true;

        private bool BlockCompletions = true;
        private readonly CompletionUiList CompletionUiList;
        private int CurrentAmountOfLines;
        private Size TextEditorTextBoxCharacterSize;
        private readonly SyntaxHighlighter Highlighter;
        public readonly PylingUnderliner Underliner;
        private bool BeginSyntaxHiglighting;

        public int FirstVisibleLineNum { get; private set; }
        public int LastVisibleLineNum { get; private set; }

        public delegate void EditorFileSavedEventArgs(TextEditor sender);
        public event EditorFileSavedEventArgs? OnFileSaved;

        public event EventHandler? ContentChanged;
        public event EventHandler? StartedPythonExecution;

        public event EventHandler<JediName>? OnClickedOnWord;

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
            string fileContent = File.ReadAllText(FilePath);

            RunFileLocalButton.IsEnabled = GlobalSettings.Default.PythonIsInstalled;
            PythonWraper.PythonExited += Python_Exited;

            CompletionUiList = new(FilePath);
            CompletionUiList.CompletionClicked += CompletionUiList_CompletionClick;
            Panel.SetZIndex(CompletionUiList, 1);
            TextEditorGrid.Children.Add(CompletionUiList);

            ReloadFile(fileContent);

            TextEditorTextBoxCharacterSize = MeasureTextBoxStringSize("A");

            Highlighter = new(TextEditorTextBoxCharacterSize, FilePath);
            Highlighter.OnHoverOverWord += Highlighter_OnHoverOverWord;
            Highlighter.OnStoppedHoveringOverWord += (s, e) => JediNameDescriber.CloseIfNotMouseOver();
            Highlighter.OnClickOnWord += (s, e) => OnClickedOnWord?.Invoke(s, e);
            JediNameDescriber.CloseWhenMouseLeaves = true;
            TextEditorGrid.Children.Add(Highlighter);

            Underliner = new(TextEditorTextBoxCharacterSize);
            TextEditorGrid.Children.Add(Underliner);

            if (EnablePythonSyntaxhighlighting && GlobalSettings.Default.JediIsUsable)
                UpdateHighlighting();

            BlockCompletions = false;
        }

        private void Python_Exited(object? sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                RunFileLocalButton.IsEnabled = true;
            });
        }

        private void Highlighter_OnHoverOverWord(object? sender, JediName jediName) {
            JediNameDescriber.OpenAsync(jediName);
            JediNameDescriber.Margin = new((double) jediName.Column * TextEditorTextBoxCharacterSize.Width + TextEditorTextBox.HorizontalOffset, (double) jediName.Line * TextEditorTextBoxCharacterSize.Height + TextEditorTextBox.VerticalOffset, 0, 0);
        }

        private void CompletionUiList_CompletionClick(object? sender, Completion e) => InsertCompletionAtCaret(e);

        public virtual void SaveFile() {
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
            } catch (Exception ex) {
                MessageBox.Show($"There was an error loading the file \"{FilePath}\"\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ReloadFile(string fileContent) => TextEditorTextBox.Text = fileContent;

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
            char keyChar;

            if (!Shortcuts.IsTheOnlyKeyPressed(e.Key))
                return;

            if (keyString.Length == 1) {
                keyChar = keyString[0];
                if (!char.IsLetter(keyChar)) {
                    CompletionUiList.Close();
                    return;
                }
            } else if (e.Key == Key.OemPeriod)
                keyString = ".";
            else {
                if (e.Key != Key.LeftCtrl)
                    CompletionUiList.Close();
                return;
            }

            if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                keyString = keyString.ToLower();

            DisplayCodeCompletionsWithExtraTextAsync(keyString);
        }

        private void InsertCompletionAtCaret(Completion completion) {

            // TODO: Fix that sometimes the completion is outdated

            if (completion.Complete is null || completion.Complete.Length == 0)
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
            // extraText must be one line!

            if (BlockCompletions || !EnableJediCompletions || !GlobalSettings.Default.JediIsUsable)
                return;

            Point caretPosition = GetCaretPosition();
            caretPosition.Y++;
            caretPosition.X += extraText.Length;

            CompletionUiList.Margin = MarginAtCaretPosition();
            await CompletionUiList.ReloadCompletionsAsync(TextEditorTextBox.Text.Insert(TextEditorTextBox.CaretIndex, extraText), caretPosition);
            CompletionUiList.SelectFirst();
        }

        protected virtual void TextEditorTextBox_TextChanged(object sender, TextChangedEventArgs e) {

            ContentIsSaved = false;
            BeginSyntaxHiglighting = true;
            ContentChanged?.Invoke(this, e);

            int textLines = Tools.CountLines(TextEditorTextBox.Text);

            if (textLines != CurrentAmountOfLines) {
                NumsTextBlock.Text = GetLineNumbers(textLines);
                CurrentAmountOfLines = textLines;
            }
        }

        private Thickness MarginAtCaretPosition() {
            Point caretPos = GetCaretPosition();
            return new(
                (caretPos.X + 0.5) * TextEditorTextBoxCharacterSize.Width,
                (caretPos.Y - (FirstVisibleLineNum == 0 ? 0 : FirstVisibleLineNum + 1) + 1) * TextEditorTextBoxCharacterSize.Height,
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
            return new Size(formattedText.Width, formattedText.Height);
        }

        private int GetCaretCol() => Tools.GetColOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex);
        private int GetCaretRow() => Tools.GetRowOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex);
        private Point GetCaretPosition() => Tools.GetPointOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex);

        private async void UpdateHighlighting() {

            while (true) {

                while (DisableAllWrapers || !BeginSyntaxHiglighting)
                    await Task.Delay(10);

                await Highlighter.HighglightTextAsync(TextEditorTextBox.Text, FirstVisibleLineNum, LastVisibleLineNum);

                BeginSyntaxHiglighting = false;
            }
        }

        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {

            FirstVisibleLineNum = (int) (e.VerticalOffset / TextEditorTextBoxCharacterSize.Height);
            LastVisibleLineNum = (int) ((e.VerticalOffset + TextEditorTextBox.ActualHeight) / TextEditorTextBoxCharacterSize.Height);

            Thickness newMargin = new(-e.HorizontalOffset, -e.VerticalOffset, 0, 0);
            Highlighter.Margin = newMargin;
            NumsTextBlock.Margin = new(0, -e.VerticalOffset, 0, 0);
            Underliner.Margin = newMargin;
            Thickness caretMargin = MarginAtCaretPosition();

            if (caretMargin.Top < 0)
                CompletionUiList.CloseTemporary();
            else {
                CompletionUiList.Margin = caretMargin;
                CompletionUiList.LoadCached();
            }

            BeginSyntaxHiglighting = true;
        }

        public void SetCaretPositioin(int line, int column) => TextEditorTextBox.CaretIndex = Tools.GetIndexOfColRow(TextEditorTextBox.Text, line, column);

        public void ScrollToCaret() {
            double verticalOffset = Tools.GetRowOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex) * TextEditorTextBoxCharacterSize.Height;
            double horizontalOffset = Tools.GetColOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex) * TextEditorTextBoxCharacterSize.Width;

            TextEditorTextBox.ScrollToVerticalOffset(verticalOffset - (ActualHeight / 2));
            TextEditorTextBox.ScrollToHorizontalOffset(horizontalOffset - (ActualHeight / 2));
        }

        public void ScrollToPosition(int line, int column) {
            double verticalOffset = line * TextEditorTextBoxCharacterSize.Height;
            double horizontalOffset = column * TextEditorTextBoxCharacterSize.Width;

            TextEditorTextBox.ScrollToVerticalOffset(verticalOffset - (ActualHeight / 2));
            TextEditorTextBox.ScrollToHorizontalOffset(horizontalOffset - (ActualHeight / 2));
        }

        private void TextEditorTextBox_LostFocus(object sender, RoutedEventArgs e) {
            CompletionUiList.Close();
            Highlighter.ForceAllButtonsToStayEnabled(false);
        }

        private void TextEditorTextBox_MouseDown(object sender, MouseButtonEventArgs e) => CompletionUiList.Close();

        private void RunFileLocalButton_Click(object sender, RoutedEventArgs e) {
            RunFileLocalButton.IsEnabled = false;
            SaveFile();
            PythonWraper.AsyncFileRunner.RunFileAsync(FilePath);
            StartedPythonExecution?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void StopAllRunningTasksButton_Click(object sender, RoutedEventArgs e) {
            PythonWraper.AsyncFileRunner.KillProcess();
            RunFileLocalButton.IsEnabled = GlobalSettings.Default.PythonIsInstalled;
        }

        private void TextEditorTextBox_PreviewKeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.LeftCtrl && Shortcuts.IsTheOnlyKeyPressed(Key.LeftCtrl)) {
                Highlighter.ForceAllButtonsToStayEnabled(false);
            }
        }

        private void TextEditorTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.LeftCtrl && Shortcuts.IsTheOnlyKeyPressed(Key.LeftCtrl)) {
                Highlighter.ForceAllButtonsToStayEnabled(true);
            }
        }
    }
}
