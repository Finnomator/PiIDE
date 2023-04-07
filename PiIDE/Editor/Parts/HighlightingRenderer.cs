using PiIDE.Options.Editor.SyntaxHighlighter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static PiIDE.Wrapers.JediWraper;
using static PiIDE.Wrapers.JediWraper.ReturnClasses;

namespace PiIDE.Editor.Parts {
    public class HighlightingRenderer {

        public readonly TextEditor Editor;
        public TextBoxWithDrawingGroup TextRenderer => Editor.TextEditorTextBox;
        public string EditorText => Editor.EditorText;
        public FormattedText RendererFormattedText => TextRenderer.VisibleTextAsFormattedText;

        public HighlightingRenderer(TextEditor textEditor) {
            Editor = textEditor;

            if (Editor.IsPythonFile) {
                TextRenderer.RemoveRenderAction(TextRenderer.DefaultRenderAction);
                SetRenderingAccordingToSettings();

                SyntaxHighlighterSettings.Default.PropertyChanged += (s, e) => {
                    // For some reason this event gets fired 3 times instead of once
                    SetRenderingAccordingToSettings();
                    TextRenderer.Render();
                };
            }
        }

        private void SetRenderingAccordingToSettings() {
            if (SyntaxHighlighterSettings.Default.HighlightBrackets)
                TextRenderer.AddRenderAction(HighlightBrackets);
            else
                TextRenderer.RemoveRenderAction(HighlightBrackets);

            if (SyntaxHighlighterSettings.Default.HighlightKeywords)
                TextRenderer.AddRenderAction(HighlightKeywords);
            else
                TextRenderer.RemoveRenderAction(HighlightKeywords);

            if (SyntaxHighlighterSettings.Default.HighlightJediNames)
                TextRenderer.AddRenderAction(HighlightJediNames);
            else
                TextRenderer.RemoveRenderAction(HighlightJediNames);

            if (SyntaxHighlighterSettings.Default.HighlightIndentation)
                TextRenderer.AddRenderAction(HighlightIndentation);
            else
                TextRenderer.RemoveRenderAction(HighlightIndentation);
        }

        private void HighlightIndentation(DrawingContext context) {
            List<Rect> rects = OptimizeIndentRectsForDrawing(SyntaxHighlighter.FindIndents(RendererFormattedText.Text));
            double lineWidth = Editor.TextEditorTextBoxCharacterSize.Width;
            foreach (Rect rect in rects) {
                int ici = ((int) ((rect.X - 2) / (lineWidth * 4))) % SyntaxHighlighter.IndentationColors.Length;
                context.DrawRectangle(SyntaxHighlighter.IndentationColors[ici], null, rect);
            }
        }

        private List<Rect> OptimizeIndentRectsForDrawing(List<SyntaxHighlighter.IndentMatch> indentMatches) {
            List<Rect> optimized = new();
            Size charSize = Editor.TextEditorTextBoxCharacterSize;
            double offset = Editor.FirstVisibleLineNum * charSize.Height;
            HashSet<int> optimizedIndexes = new();

            for (int i = 0; i < indentMatches.Count; i++) {

                if (optimizedIndexes.Contains(i))
                    continue;

                SyntaxHighlighter.IndentMatch currentMatch = indentMatches[i];
                Rect optimizedRect = new(currentMatch.Column * charSize.Width + 2, offset + currentMatch.Row * charSize.Height, 1, charSize.Height);

                for (int j = i + 1; j < indentMatches.Count; ++j) {

                    SyntaxHighlighter.IndentMatch nextMatch = indentMatches[j];
                    if (nextMatch.Column == currentMatch.Column && nextMatch.Row == currentMatch.Row + 1) {
                        optimizedRect.Height += charSize.Height;
                        currentMatch = nextMatch;
                        optimizedIndexes.Add(j);
                    }
                }

                optimized.Add(optimizedRect);
            }

            return optimized;
        }

        private void HighlightKeywords(DrawingContext context) {

            string visibleText = RendererFormattedText.Text;

            // The highlighting order is critical, so it overlaps the highlighted strings in comments

            foreach (Match keyword in SyntaxHighlighter.FindKeywords(visibleText))
                RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.Keyword, keyword.Index, keyword.Length);

            foreach (Match number in SyntaxHighlighter.FindNumbers(visibleText))
                RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.Number, number.Index, number.Length);

            foreach (Match stringMatch in SyntaxHighlighter.FindStrings(visibleText))
                RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.String, stringMatch.Index, stringMatch.Length);

            foreach (Match comment in SyntaxHighlighter.FindComments(visibleText))
                RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.Comment, comment.Index, comment.Length);
        }

        private void HighlightJediNames(DrawingContext context) {

            string visibleText = RendererFormattedText.Text;

            Script script = Script.MakeScript(visibleText, Editor.FilePath);

            Name[] jediNames = SyntaxHighlighter.FindJediNames(script);

            int[] cols = jediNames.Select(x => x.Column).Cast<int>().ToArray();
            int[] rows = jediNames.Select(x => x.Line - 1).Cast<int>().ToArray();
            int[] jediIndexes = visibleText.GetIndexesOfColRows(rows, cols);

            for (int i = 0; i < jediNames.Length; i++) {
                Name jediName = jediNames[i];
                int index = jediIndexes[i];

                RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.GetBrush(jediName.Type), index, jediName.Name.Length);
            }
        }

        private void HighlightBrackets(DrawingContext context) {

            List<SyntaxHighlighter.BracketMatch> brackets = SyntaxHighlighter.FindBrackets(EditorText);
            int fvl = Editor.FirstVisibleLineNum;
            int lvl = Editor.LastVisibleLineNum;
            int firstVisibleIndex = Tools.GetIndexOfColRow(EditorText, fvl, 0);

            if (firstVisibleIndex == -1)
                return;

            for (int i = 0; i < brackets.Count; ++i) {

                SyntaxHighlighter.BracketMatch bracket = brackets[i];

                if (bracket.Row < fvl)
                    continue;
                if (bracket.Row >= lvl)
                    break;

                int bci = Math.Abs(bracket.BracketIndex % SyntaxHighlighter.BracketColors.Length);

                RendererFormattedText.SetForegroundBrush(SyntaxHighlighter.BracketColors[bci], bracket.Index - firstVisibleIndex, 1);
            }
        }
    }
}
