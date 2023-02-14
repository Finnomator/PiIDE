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

        public TextEditorCore(TextEditor textEditor) {
            Editor = textEditor;
            IsHitTestVisible = false;
        }

        public async void UpdateTextAsync() {


            for (int i = 0; i < 50 && OldVisibleText == VisibleText; i++)
                await Task.Delay(10);

            bool textChanged = OldVisibleText != VisibleText;

            string visibleText = VisibleText;
            string editorText = EditorText;
            int fvl = Editor.FirstVisibleLineNum;
            int lvl = Editor.LastVisibleLineNum;

            if (visibleText == "") {
                DrawingContext c = drawingGroup.Open();
                c.Close();
                return;
            }

            // TODO: Add something like a queue

            if (IsBusy)
                return;
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

            Match[] keywordMatches = SyntaxHighlighter.FindKeywords(visibleText);
            for (int i = 0; i < keywordMatches.Length; i++) {
                Match match = keywordMatches[i];
                formattedText.SetForegroundBrush(TypeColors.Keyword, match.Index, match.Length);
            }

            Script script = await Script.MakeScript(editorText, Editor.FilePath);

            if (textChanged || CachedJediNames is null)
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
                try {
                    formattedText.SetForegroundBrush(TypeColors.TypeToColor(name.Type), index, name.Name.Length);
                } catch {
                    //TODO: fix index out of range
                }
            }


            OldVisibleText = visibleText;
            DrawingContext context = drawingGroup.Open();
            context.DrawText(formattedText, new(2, 0));
            context.Close();

            IsBusy = false;
        }

        protected override void OnRender(DrawingContext drawingContext) {
            UpdateTextAsync();
            drawingContext.DrawDrawing(drawingGroup);
        }
    }
}
