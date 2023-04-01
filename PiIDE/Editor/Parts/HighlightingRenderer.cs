using PiIDE.Options.Editor.SyntaxHighlighter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
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

            string visibleText = RendererFormattedText.Text;

            List<SyntaxHighlighter.BracketMatch> brackets = SyntaxHighlighter.FindBrackets(visibleText);

            for (int i = 0; i < brackets.Count; ++i) {

                SyntaxHighlighter.BracketMatch bracket = brackets[i];

                int bci = Math.Abs(bracket.BracketIndex % SyntaxHighlighter.BracketColors.Length);

                Brush brush = new SolidColorBrush(SyntaxHighlighter.BracketColors[bci]);

                RendererFormattedText.SetForegroundBrush(brush, bracket.Index, 1);
            }
        }
    }
}
