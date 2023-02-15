using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static PiIDE.Wrapers.JediWraper;

namespace PiIDE.Editor.Parts {
    public class TextEditorCore : UIElement {

        private readonly DrawingGroup drawingGroup = new();
        private readonly TextEditor Editor;
        private TextBox EditorBox => Editor.TextEditorTextBox;
        private string EditorText => Editor.TextEditorTextBox.Text;
        private string VisibleText => Editor.VisibleText;
        private string OldVisibleText = "";
        private ReturnClasses.Name[]? CachedJediNames;
        private bool IsBusy;
        private bool GotNewerRequest;

        public event EventHandler? StartedHighlighting;
        public event EventHandler? FinishedHighlighting;


        public TextEditorCore(TextEditor textEditor) {
            Editor = textEditor;
            IsHitTestVisible = false;
        }

        public async void UpdateTextAsync(HighlightingMode highlightingMode, HighlightingPerformanceMode performanceMode) {

            for (int i = 0; i < 50 && OldVisibleText == VisibleText; i++)
                await Task.Delay(10);

            if (VisibleText == "") {
                DrawingContext c = drawingGroup.Open();
                c.Close();
                return;
            }

            if (IsBusy) {
                GotNewerRequest = true;
                return;
            }

            StartedHighlighting?.Invoke(this, EventArgs.Empty);

            // this is used because the EditorText could change while waiting for jedi to finish
            string visibleText = VisibleText;

            IsBusy = true;

            FormattedText formattedText = new(
                textToFormat: visibleText,
                culture: CultureInfo.GetCultureInfo("en-us"),
                flowDirection: FlowDirection.LeftToRight,
                typeface: new Typeface(EditorBox.FontFamily.Source),
                emSize: EditorBox.FontSize,
                foreground: Brushes.Black,
                pixelsPerDip: VisualTreeHelper.GetDpi(this).PixelsPerDip
            );

            switch (highlightingMode) {
                case HighlightingMode.JediAndKeywords:
                    HighlightKeywords(formattedText, visibleText);
                    await HighlightJediNamesAsync(formattedText, visibleText, performanceMode);
                    break;
                case HighlightingMode.JediOnly:
                    await HighlightJediNamesAsync(formattedText, visibleText, performanceMode);
                    break;
                case HighlightingMode.KeywordsOnly:
                    HighlightKeywords(formattedText, visibleText);
                    break;
                default:
                    throw new Exception();
            }

            OldVisibleText = visibleText;
            DrawingContext context = drawingGroup.Open();
            context.DrawText(formattedText, new(2, 0));
            context.Close();

            IsBusy = false;

            if (GotNewerRequest) {
                UpdateTextAsync(highlightingMode, performanceMode);
                GotNewerRequest = false;
            }

            FinishedHighlighting?.Invoke(this, EventArgs.Empty);
        }

        private static void HighlightKeywords(FormattedText formattedText, string visibleText) {
            Match[] keywordMatches = SyntaxHighlighter.FindKeywords(visibleText);
            for (int i = 0; i < keywordMatches.Length; i++) {
                Match match = keywordMatches[i];
                formattedText.SetForegroundBrush(TypeColors.Keyword, match.Index, match.Length);
            }
        }

        private async Task HighlightJediNamesAsync(FormattedText formattedText, string visibleText, HighlightingPerformanceMode performanceMode) {
            if (performanceMode == HighlightingPerformanceMode.Normal)
                await HighlightJediNamesNormalAsync(formattedText, visibleText);
            else if (performanceMode == HighlightingPerformanceMode.Performance)
                await HighlightJediNamesPerformanceAsync(formattedText, visibleText);
        }

        private async Task HighlightJediNamesNormalAsync(FormattedText formattedText, string visibleText) {

            int fvl = Editor.FirstVisibleLineNum;
            int lvl = Editor.LastVisibleLineNum;

            Script script = await Script.MakeScript(EditorText, Editor.FilePath);

            if (OldVisibleText != VisibleText || CachedJediNames is null)
                CachedJediNames = await SyntaxHighlighter.FindJediNames(script);

            ReturnClasses.Name[] visibleJediNames = CachedJediNames.Where(x => x.Line > fvl && x.Line <= lvl).ToArray();

            int[] cols = new int[visibleJediNames.Length];
            int[] rows = new int[visibleJediNames.Length];

            for (int i = 0; i < cols.Length; ++i) {
                cols[i] = (int) visibleJediNames[i].Column;
                rows[i] = (int) visibleJediNames[i].Line - fvl - 1;
            }

            int[] jediIndexes = Tools.GetIndexesOfColRows(visibleText, rows, cols);

            for (int i = 0; i < visibleJediNames.Length; ++i) {
                ReturnClasses.Name name = visibleJediNames[i];
                int index = jediIndexes[i];
                formattedText.SetForegroundBrush(TypeColors.TypeToColor(name.Type), index, name.Name.Length);
            }
        }

        private async Task HighlightJediNamesPerformanceAsync(FormattedText formattedText, string visibleText) {

            Script script = await Script.MakeScript(visibleText, Editor.FilePath);

            if (OldVisibleText != VisibleText || CachedJediNames is null)
                CachedJediNames = await SyntaxHighlighter.FindJediNames(script);

            ReturnClasses.Name[] visibleJediNames = CachedJediNames;

            int[] cols = new int[visibleJediNames.Length];
            int[] rows = new int[visibleJediNames.Length];

            for (int i = 0; i < cols.Length; ++i) {
                cols[i] = (int) visibleJediNames[i].Column;
                rows[i] = (int) visibleJediNames[i].Line - 1;
            }

            int[] jediIndexes = Tools.GetIndexesOfColRows(visibleText, rows, cols);

            for (int i = 0; i < visibleJediNames.Length; ++i) {
                ReturnClasses.Name name = visibleJediNames[i];
                int index = jediIndexes[i];
                formattedText.SetForegroundBrush(TypeColors.TypeToColor(name.Type), index, name.Name.Length);
            }
        }

        protected override void OnRender(DrawingContext drawingContext) {
            UpdateTextAsync(TextEditor.HighlightingMode, TextEditor.HighlightingPerformanceMode);
            drawingContext.DrawDrawing(drawingGroup);
        }
    }
}
