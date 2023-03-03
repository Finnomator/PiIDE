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

        private readonly DrawingGroup DrawingGroup = new();
        private readonly TextEditor Editor;
        private TextBox EditorBox => Editor.TextEditorTextBox;
        private string EditorText => Editor.TextEditorTextBox.Text;
        private string VisibleText => Editor.VisibleText;
        private Size CharSize => Editor.TextEditorTextBoxCharacterSize;
        private string OldVisibleText = "";
        private ReturnClasses.Name[]? CachedJediNames;
        private bool IsBusy;
        private bool GotNewerRequest;
        private HighlightingPerformanceMode OldHighlightingPerformanceMode;
        private MatchCollection? OldSearchResult;

        public FormattedText? CurrentHighlighting { get; private set; }

        public event EventHandler? StartedHighlighting;
        public event EventHandler? FinishedHighlighting;

        public TextEditorCore(TextEditor textEditor) {
            Editor = textEditor;
            IsHitTestVisible = false;
            OldHighlightingPerformanceMode = TextEditor.HighlightingPerformanceMode;

            Editor.MainScrollViewer.ScrollChanged += (s, e) => UpdateView(OldSearchResult);
            Editor.TextSearchBox.SearchChanged += (s, e) => {
                MatchCollection? matches = null;
                if (e is not null) {
                    matches = e.Matches(EditorText);
                    Editor.TextSearchBox.SetSearchResults(matches.Count);
                }
                UpdateView(matches);
            };
            Editor.TextEditorTextBox.TextChanged += (s, e) => {
                MatchCollection? matches = Editor.TextSearchBox.Searcher?.Matches(EditorText);
                if (matches is not null)
                    Editor.TextSearchBox.SetSearchResults(matches.Count);
                UpdateView(matches);
            };
            Editor.TextSearchBox.SelectedResultChanged += (s, e) => {
                if (OldSearchResult is null)
                    return;
                (int col, int row) = EditorText.GetPointOfIndex(OldSearchResult[e].Index);
                UpdateView(OldSearchResult);
                Editor.ScrollToPosition(row, col);
            };
            GlobalSettings.Default.PropertyChanged += (s, e) => UpdateView(OldSearchResult);
            ColorResources.HighlighterColors.ColorChanged += (s, e) => UpdateView(OldSearchResult);
        }

        private async void UpdateView(MatchCollection? searchResult) {

            if (Editor.DisableAllWrapers)
                return;

            for (int i = 0; i < 50 && OldVisibleText == VisibleText; i++)
                await Task.Delay(10);

            if (VisibleText == "") {
                try {
                    DrawingGroup.Open().Close();
                } catch (InvalidOperationException) { }
                return;
            }

            if (IsBusy) {
                GotNewerRequest = true;
                return;
            }
            IsBusy = true;

            // this is used because the EditorText could change while waiting for jedi to finish
            string filePath = Editor.FilePath;
            string visibleText = VisibleText;
            FormattedText formattedText = GetFormattedText(visibleText);

            DrawingContext context = DrawingGroup.Open();

            if (searchResult is not null) {
                ApplySearchResults(context, searchResult);
                OldSearchResult = searchResult;
            }

            await ApplyHighlighting(context, formattedText, filePath, TextEditor.HighlightingMode, TextEditor.HighlightingPerformanceMode);

            context.Close();

            OldVisibleText = visibleText;
            OldHighlightingPerformanceMode = TextEditor.HighlightingPerformanceMode;

            IsBusy = false;

            if (GotNewerRequest) {
                UpdateView(searchResult);
                GotNewerRequest = false;
            }
        }

        private void ApplySearchResults(DrawingContext context, MatchCollection searchResult) {

            Brush foundBrush = (Brush) Tools.BrushConverter.ConvertFrom("#40FFFFFF")!;
            Brush selectedBrush = (Brush) Tools.BrushConverter.ConvertFrom("#70FFFF00")!;

            int fvl = Editor.FirstVisibleLineNum;
            int lvl = Editor.LastVisibleLineNum;

            (int col, int row)[] points = EditorText.GetPointsOfIndexes(searchResult.Select(x => x.Index).ToArray());

            Size charSize = CharSize;

            for (int i = 0; i < searchResult.Count; i++) {
                Match match = searchResult[i];
                (int col, int row) = points[i];

                if (row < fvl)
                    continue;
                if (row > lvl)
                    break;

                context.DrawRectangle(Editor.TextSearchBox.ResultNo == i ? selectedBrush : foundBrush, null, new(col * charSize.Width + 2, (row - fvl) * charSize.Height, match.Length * charSize.Width, charSize.Height));
            }
        }

        private async Task ApplyHighlighting(DrawingContext context, FormattedText formattedText, string filePath, HighlightingMode highlightingMode, HighlightingPerformanceMode performanceMode) {

            string visibleText = formattedText.Text;

            StartedHighlighting?.Invoke(this, EventArgs.Empty);

            switch (highlightingMode) {
                case HighlightingMode.JediAndKeywords:
                    HighlightKeywords(formattedText);
                    await HighlightJediNamesAsync(formattedText, filePath, performanceMode);
                    break;
                case HighlightingMode.JediOnly:
                    await HighlightJediNamesAsync(formattedText, filePath, performanceMode);
                    break;
                case HighlightingMode.KeywordsOnly:
                    HighlightKeywords(formattedText);
                    break;
                default:
                    break;
            }

            DrawHighlighting(context, formattedText);

            FinishedHighlighting?.Invoke(this, EventArgs.Empty);
        }

        private FormattedText GetFormattedText(string text) {
            return new(
                textToFormat: text,
                culture: CultureInfo.GetCultureInfo("en-us"),
                flowDirection: FlowDirection.LeftToRight,
                typeface: new Typeface(EditorBox.FontFamily.Source),
                emSize: EditorBox.FontSize,
                foreground: Brushes.White,
                pixelsPerDip: VisualTreeHelper.GetDpi(EditorBox).PixelsPerDip
            );
        }

        private void DrawHighlighting(DrawingContext drawingContext, FormattedText formattedText) {
            drawingContext.DrawText(formattedText, new(2, 0));
            CurrentHighlighting = formattedText;
        }

        private static void HighlightKeywords(FormattedText formattedText) {
            string visibleText = formattedText.Text;

            Match[] keywordMatches = SyntaxHighlighter.FindKeywords(visibleText);
            Match[] commentMatches = SyntaxHighlighter.FindComments(visibleText);
            Match[] stringMatches = SyntaxHighlighter.FindStrings(visibleText);
            Match[] numberMatches = SyntaxHighlighter.FindNumbers(visibleText);

            // The highlighting order is critical, so it overlaps the highlighted strings in comments

            foreach (Match keyword in keywordMatches)
                formattedText.SetForegroundBrush(ColorResources.HighlighterColors.Keyword, keyword.Index, keyword.Length);

            foreach (Match number in numberMatches)
                formattedText.SetForegroundBrush(ColorResources.HighlighterColors.Number, number.Index, number.Length);

            foreach (Match stringMatch in stringMatches)
                formattedText.SetForegroundBrush(ColorResources.HighlighterColors.String, stringMatch.Index, stringMatch.Length);

            foreach (Match comment in commentMatches)
                formattedText.SetForegroundBrush(ColorResources.HighlighterColors.Comment, comment.Index, comment.Length);
        }

        private async Task HighlightJediNamesAsync(FormattedText formattedText, string filePath, HighlightingPerformanceMode performanceMode) {
            if (performanceMode == HighlightingPerformanceMode.Normal)
                await HighlightJediNamesNormalAsync(formattedText, filePath);
            else if (performanceMode == HighlightingPerformanceMode.Performance)
                await HighlightJediNamesPerformanceAsync(formattedText, filePath);
        }

        private async Task HighlightJediNamesNormalAsync(FormattedText formattedText, string filePath) {

            string visibleText = formattedText.Text;

            int fvl = Editor.FirstVisibleLineNum;
            int lvl = Editor.LastVisibleLineNum;

            if (OldVisibleText != visibleText || CachedJediNames is null || OldHighlightingPerformanceMode == HighlightingPerformanceMode.Performance) {
                Script script = await Script.MakeScript(EditorText, filePath);
                CachedJediNames = await SyntaxHighlighter.FindJediNames(script);
            }

            ReturnClasses.Name[] visibleJediNames = CachedJediNames.Where(x => x.Line > fvl && x.Line <= lvl).ToArray();

            int[] cols = new int[visibleJediNames.Length];
            int[] rows = new int[visibleJediNames.Length];

            for (int i = 0; i < cols.Length; ++i) {
                cols[i] = (int) visibleJediNames[i].Column!;
                rows[i] = (int) visibleJediNames[i].Line! - fvl - 1;
            }

            int[] jediIndexes = Tools.GetIndexesOfColRows(visibleText, rows, cols);

            for (int i = 0; i < visibleJediNames.Length; ++i) {
                ReturnClasses.Name name = visibleJediNames[i];
                int index = jediIndexes[i];
                formattedText.SetForegroundBrush(ColorResources.HighlighterColors.GetBrush(name.Type), index, name.Name.Length);
            }
        }

        private async Task HighlightJediNamesPerformanceAsync(FormattedText formattedText, string filePath) {
            string visibleText = formattedText.Text;

            if (OldVisibleText != visibleText || CachedJediNames is null || OldHighlightingPerformanceMode == HighlightingPerformanceMode.Normal) {
                Script script = await Script.MakeScript(visibleText, filePath);
                CachedJediNames = await SyntaxHighlighter.FindJediNames(script);
            }

            ReturnClasses.Name[] visibleJediNames = CachedJediNames;

            int[] cols = new int[visibleJediNames.Length];
            int[] rows = new int[visibleJediNames.Length];

            for (int i = 0; i < cols.Length; ++i) {
                cols[i] = (int) visibleJediNames[i].Column!;
                rows[i] = (int) visibleJediNames[i].Line! - 1;
            }

            int[] jediIndexes = Tools.GetIndexesOfColRows(visibleText, rows, cols);

            for (int i = 0; i < visibleJediNames.Length; ++i) {
                ReturnClasses.Name name = visibleJediNames[i];
                int index = jediIndexes[i];
                formattedText.SetForegroundBrush(ColorResources.HighlighterColors.GetBrush(name.Type), index, name.Name.Length);
            }
        }

        protected override void OnRender(DrawingContext drawingContext) {
            UpdateView(null);
            drawingContext.DrawDrawing(DrawingGroup);
        }
    }
}
