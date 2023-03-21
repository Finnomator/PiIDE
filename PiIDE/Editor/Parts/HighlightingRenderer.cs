using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using static PiIDE.Wrapers.JediWraper;
using static PiIDE.Wrapers.JediWraper.ReturnClasses;

namespace PiIDE.Editor.Parts {
    public class HighlightingRenderer {

        private readonly TextEditor Editor;
        private TextBoxWithDrawingGroup TextRenderer => Editor.TextEditorTextBox;
        private string EditorText => Editor.EditorText;
        private FormattedText RendererFormattedText => TextRenderer.VisibleTextAsFormattedText;

        public HighlightingRenderer(TextEditor textEditor) {
            Editor = textEditor;
            TextRenderer.RemoveRenderAction(TextRenderer.DefaultRenderAction);
            TextRenderer.AddRenderAction(HighlightBrackets);
            TextRenderer.AddRenderAction(HighlightKeywords);
            TextRenderer.AddRenderAction(HighlightJediNames);
        }

        private void HighlightKeywords(DrawingContext context) {

            string visibleText = RendererFormattedText.Text;

            Match[] keywordMatches = SyntaxHighlighter.FindKeywords(visibleText);
            Match[] commentMatches = SyntaxHighlighter.FindComments(visibleText);
            Match[] stringMatches = SyntaxHighlighter.FindStrings(visibleText);
            Match[] numberMatches = SyntaxHighlighter.FindNumbers(visibleText);

            // The highlighting order is critical, so it overlaps the highlighted strings in comments

            foreach (Match keyword in keywordMatches)
                RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.Keyword, keyword.Index, keyword.Length);

            foreach (Match number in numberMatches)
                RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.Number, number.Index, number.Length);

            foreach (Match stringMatch in stringMatches)
                RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.String, stringMatch.Index, stringMatch.Length);

            foreach (Match comment in commentMatches)
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
