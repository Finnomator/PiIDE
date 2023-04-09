using PiIDE.Options.Editor.SyntaxHighlighter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using static PiIDE.Wrappers.JediWrapper;
using static PiIDE.Wrappers.JediWrapper.ReturnClasses;

namespace PiIDE.Editor.Parts;

public class HighlightingRenderer {

    public readonly TextEditor Editor;
    public TextBoxWithDrawingGroup TextRenderer => Editor.TextEditorTextBox;
    public string EditorText => Editor.EditorText;
    public FormattedText RendererFormattedText => TextRenderer.VisibleTextAsFormattedText!;

    public HighlightingRenderer(TextEditor textEditor) {
        Editor = textEditor;

        if (!Editor.IsPythonFile)
            return;
        
        TextRenderer.RemoveRenderAction(TextRenderer.DefaultRenderAction);
        SetRenderingAccordingToSettings();

        SyntaxHighlighterSettings.Default.PropertyChanged += (_, _) => {
            // For some reason this event gets fired 3 times instead of once
            SetRenderingAccordingToSettings();
            TextRenderer.Render();
        };
    }

    private void SetRenderingAccordingToSettings() {
        if (SyntaxHighlighterSettings.Default.HighlightBrackets)
            TextRenderer.AddRenderAction(1, HighlightBrackets);
        else
            TextRenderer.RemoveRenderAction(HighlightBrackets);

        if (SyntaxHighlighterSettings.Default.HighlightKeywords)
            TextRenderer.AddRenderAction(3, HighlightKeywords);
        else
            TextRenderer.RemoveRenderAction(HighlightKeywords);

        if (SyntaxHighlighterSettings.Default.HighlightJediNames)
            TextRenderer.AddRenderAction(0, HighlightJediNames);
        else
            TextRenderer.RemoveRenderAction(HighlightJediNames);

        if (SyntaxHighlighterSettings.Default.HighlightIndentation)
            TextRenderer.AddRenderAction(2, HighlightIndentation);
        else
            TextRenderer.RemoveRenderAction(HighlightIndentation);
    }

    private void HighlightIndentation(DrawingContext context) {
        List<Rect> rects = OptimizeIndentRectsForDrawing(SyntaxHighlighter.FindIndents(RendererFormattedText.Text));
        double lineWidth = Editor.TextEditorTextBoxCharacterSize.Width;
        foreach (Rect rect in rects) {
            int ici = (int) ((rect.X - 2) / (lineWidth * 4)) % SyntaxHighlighter.IndentationColors.Length;
            context.DrawRectangle(SyntaxHighlighter.IndentationColors[ici], null, rect);
        }
    }

    private List<Rect> OptimizeIndentRectsForDrawing(List<SyntaxHighlighter.IndentMatch> indentMatches) {
        List<Rect> optimized = new();
        Size charSize = Editor.TextEditorTextBoxCharacterSize;
        double offset = Editor.GetFirstVisibleLineNum() * charSize.Height;
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

        // The highlighting order is critical, so it overlaps the highlighted strings in comments

        foreach (Match keyword in SyntaxHighlighter.FindKeywords(RendererFormattedText.Text))
            RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.Keyword, keyword.Index, keyword.Length);

        foreach (Match number in SyntaxHighlighter.FindNumbers(RendererFormattedText.Text))
            RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.Number, number.Index, number.Length);

        foreach (Match stringMatch in SyntaxHighlighter.FindSingleQuotedStrings(RendererFormattedText.Text))
            RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.String, stringMatch.Index, stringMatch.Length);

        HighlightTripleQuotedStrings();

        foreach (Match comment in SyntaxHighlighter.FindComments(RendererFormattedText.Text))
            RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.Comment, comment.Index, comment.Length);
    }

    private void HighlightTripleQuotedStrings() {

        MatchCollection allStringMatches = SyntaxHighlighter.FindTripleQuotedStrings(EditorText);
        (int firstVisibleIndex, int lastVisibleIndex) = Editor.GetFirstAndLastVisibleIndex();

        if (firstVisibleIndex == -1 || lastVisibleIndex == -1)
            return;

        for (int i = 0; i < allStringMatches.Count; i++) {
            Match stringMatch = allStringMatches[i];

            if (stringMatch.Index + stringMatch.Length < firstVisibleIndex)
                continue;
            if (stringMatch.Index >= lastVisibleIndex)
                break;

            int highlightStartIdx = stringMatch.Index - firstVisibleIndex;
            int highlightEndIdx = highlightStartIdx + stringMatch.Length;

            if (highlightStartIdx < firstVisibleIndex - firstVisibleIndex)
                highlightStartIdx = 0;

            if (highlightEndIdx > lastVisibleIndex - firstVisibleIndex)
                highlightEndIdx = RendererFormattedText.Text.Length;

            RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.String, highlightStartIdx, highlightEndIdx - highlightStartIdx);
        }
    }

    private void HighlightJediNames(DrawingContext context) {

        Script.MakeScript(RendererFormattedText.Text, Editor.FilePath);
        Name[] jediNames = SyntaxHighlighter.FindJediNames();

        int[] cols = jediNames.Select(x => x.Column).Cast<int>().ToArray();
        int[] rows = jediNames.Select(x => x.Line - 1).Cast<int>().ToArray();
        int[] jediIndexes = RendererFormattedText.Text.GetIndexesOfColRows(rows, cols);

        for (int i = 0; i < jediNames.Length; i++) {
            Name jediName = jediNames[i];
            int index = jediIndexes[i];

            RendererFormattedText.SetForegroundBrush(ColorResources.HighlighterColors.GetBrush(jediName.Type), index, jediName.Name.Length);
        }
    }

    private void HighlightBrackets(DrawingContext context) {

        List<SyntaxHighlighter.BracketMatch> brackets = SyntaxHighlighter.FindBrackets(EditorText);
        (int fvl, int lvl) = Editor.GetFirstAndLastVisibleLineNum();
        int firstVisibleIndex = Editor.GetFirstVisibleIndex();

        if (firstVisibleIndex == -1)
            return;

        foreach (SyntaxHighlighter.BracketMatch bracket in brackets) {
            if (bracket.Row < fvl)
                continue;
            if (bracket.Row >= lvl - 1)
                break;

            int bci = Math.Abs(bracket.BracketIndex % SyntaxHighlighter.BracketColors.Length);

            RendererFormattedText.SetForegroundBrush(SyntaxHighlighter.BracketColors[bci], bracket.Index - firstVisibleIndex, 1);
        }
    }
}