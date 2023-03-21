using System;
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
        private FormattedText RendererFormattedText => TextRenderer.TextAsFormattedText;

        public HighlightingRenderer(TextEditor textEditor) {
            Editor = textEditor;
            //TextRenderer.RemoveRenderAction(TextRenderer.DefaultRenderAction);
            TextRenderer.AddRenderAction(HighlightKeywords);
            TextRenderer.AddAsyncRenderAction(HighlightJediNamesAsync);
        }

        private void HighlightKeywords(DrawingContext context) {

            Match[] keywordMatches = SyntaxHighlighter.FindKeywords(EditorText);
            Match[] commentMatches = SyntaxHighlighter.FindComments(EditorText);
            Match[] stringMatches = SyntaxHighlighter.FindStrings(EditorText);
            Match[] numberMatches = SyntaxHighlighter.FindNumbers(EditorText);

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

        private async Task HighlightJediNamesAsync(DrawingContext context) {

            Script script = await Script.MakeScript(EditorText, Editor.FilePath);

            Name[] jediNames = await SyntaxHighlighter.FindJediNames(script);

            int[] cols = new int[jediNames.Length];
            int[] rows = new int[jediNames.Length];

            for (int i = 0; i < cols.Length; ++i) {
                cols[i] = (int) jediNames[i].Column!;
                rows[i] = (int) jediNames[i].Line! - 1;
            }

            int[] jediIndexes = EditorText.GetIndexesOfColRows(rows, cols);

            for (int i = 0; i < jediNames.Length; i++) {
                Name jediName = jediNames[i];
                int index = jediIndexes[i];
                RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.GetBrush(jediName.Type), index, jediName.Name.Length);
            }
        }
    }
}
