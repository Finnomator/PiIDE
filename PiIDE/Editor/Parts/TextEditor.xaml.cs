using PiIDE.Editor.Parts;
using PiIDE.Wrapers;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
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

        private readonly CompletionUiList CompletionUiList;
        private int CurrentAmountOfLines;
        private Size TextEditorTextBoxCharacterSize;
        private readonly TextEditorCore EditorCore;
        private readonly PylingUnderliner Underliner;
        protected int AutoSaveDelaySeconds = 5;
        protected bool DoAutoSaves = true;

        public bool DisableAllWrapers { get; set; }
        public bool ContentIsSaved { get; private set; } = true;
        public bool ContentLoaded { get; private set; }

        public string EditorText => TextEditorTextBox.Text;
        public int FirstVisibleLineNum => (int) Math.Round(MainScrollViewer.VerticalOffset / TextEditorTextBoxCharacterSize.Height);
        public int LastVisibleLineNum {
            get {
                int lines = (int) ((MainScrollViewer.VerticalOffset + OuterTextGrid.ActualHeight) / TextEditorTextBoxCharacterSize.Height) + 1;
                return Tools.CountLines(EditorText) < lines ? TextEditorTextBox.LineCount : lines;
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
            TextEditorTextBoxCharacterSize = MeasureTextBoxStringSize("A");

            Loaded += delegate {
                if (!ContentLoaded)
                    ReloadFile();
            };

            RunFileLocalButton.IsEnabled = GlobalSettings.Default.PythonIsInstalled;
            PythonWraper.PythonExited += Python_Exited;

            // Completion suggestions stuff
            CompletionUiList = new(FilePath);
            CompletionUiList.CompletionClicked += CompletionUiList_CompletionClick;
            TextEditorGrid.Children.Add(CompletionUiList);

            // Syntax highlighter
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
            Grid.SetColumn(EditorCore, 2);
            OuterTextGrid.Children.Add(EditorCore);

            // Pylint underlining stuff
            Underliner = new(TextEditorTextBoxCharacterSize);
            TextEditorGrid.Children.Add(Underliner);

            AutoSave();
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
                    } else {
                        if (TextEditorTextBox.CaretIndex > 0) {
                            char charInFrontOfCaret = EditorText[TextEditorTextBox.CaretIndex - 1];
                            int caretRow = GetCaretRow();

                            int indentAmount = GetIndentOfLine(caretRow);

                            InsertAtCaretAndMoveCaret("\r\n" + new string(' ', indentAmount + (charInFrontOfCaret == ':' ? 4 : 0)));

                            e.Handled = true;
                        }
                    }
                    break;
                case Key.Tab:
                    if (CompletionUiList.SelectedAnIndex) {
                        InsertCompletionAtCaret(CompletionUiList.SelectedCompletion);
                        CompletionUiList.Close();
                    } else {
                        // TODO: replace with a variable option
                        int spaceToFillIndent = 4 - GetCaretPosition().col % 4;
                        if (spaceToFillIndent == 0)
                            spaceToFillIndent = 4;
                        InsertAtCaretAndMoveCaret(new string(' ', spaceToFillIndent));
                    }
                    e.Handled = true;
                    break;
                case Key.S:
                    if (Shortcuts.IsShortcutPressed(Shortcut.SaveFile)) {
                        SaveFile(true);
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

            DisplayCodeCompletionsWithExtraText(keyString);
        }

        private void InsertCompletionAtCaret(Completion completion) {

            // TODO: Fix that sometimes the completion is outdated

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

        private void DisplayCodeCompletionsAsync() => DisplayCodeCompletionsWithExtraText("");

        private void DisplayCodeCompletionsWithExtraText(string extraText) {

            // This method is intended to be used when the user presses a key and the input is not in textbox yet
            // extraText must be one line!

            if (!EnableJediCompletions || !GlobalSettings.Default.JediIsUsable)
                return;

            (int col, int row) caretPosition = GetCaretPosition();
            caretPosition.row++;
            caretPosition.col += extraText.Length;

            Thickness marginAtCaretPos = MarginAtCaretPosition();
            CompletionUiList.Margin = marginAtCaretPos;

            CompletionUiList.ReloadCompletionsAsync(TextEditorTextBox.Text.Insert(TextEditorTextBox.CaretIndex, extraText), caretPosition, true);
        }

        protected virtual void TextEditorTextBox_TextChanged(object sender, TextChangedEventArgs e) {

            ContentIsSaved = !ContentLoaded;
            ContentChanged?.Invoke(this, e);
            UpdateHighlighting();

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
            return new Size(formattedText.Width, formattedText.Height);
        }

        private async void AutoSave() {
            while (DoAutoSaves) {
                await Task.Delay(AutoSaveDelaySeconds * 1000);
                if (!ContentIsSaved)
                    SaveFile(false);
            }
        }

        private int GetCaretRow() => Tools.GetRowOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex);
        private (int col, int row) GetCaretPosition() => Tools.GetPointOfIndex(TextEditorTextBox.Text, TextEditorTextBox.CaretIndex);

        private void UpdateHighlighting() {

            if (DisableAllWrapers)
                return;

            EditorCore.UpdateTextAsync();
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
            SaveFile(true);
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
                AmountOfErrorsTextBlock.Text = pylintMessages.Where(x => x.Type == "error").Count().ToString();
                AmountOfWarningsTextBlock.Text = pylintMessages.Where(x => x.Type == "warning").Count().ToString();
            }
        }

        public void UpdatePylint() {
            if (EnablePylinting && GlobalSettings.Default.PylintIsUsable)
                Underliner.UpdateUnderline(FirstVisibleLineNum, LastVisibleLineNum);
        }

        private void MainScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            e.Handled = true;
            MainScrollViewer.ScrollToVerticalOffset((FirstVisibleLineNum - e.Delta / 60) * TextEditorTextBoxCharacterSize.Height);
        }
    }
}
