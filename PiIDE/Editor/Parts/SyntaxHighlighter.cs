using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using static PiIDE.Wrapers.JediWraper;
using JediName = PiIDE.Wrapers.JediWraper.ReturnClasses.Name;

namespace PiIDE.Editor.Parts;

public static partial class SyntaxHighlighter {

    private static readonly Regex KeywordsRx = new(@$"\b({string.Join('|', Tools.PythonKeywords)})\b", RegexOptions.Compiled);
    private static readonly Regex DefaultSingleQuotedStringsRx = DefaultSingleQuotedStringsRegex();
    private static readonly Regex DefaultTripleQuotedStringsRx = DefaultTripleQuotedStringsRegex();
    private static readonly Regex CommentsRx = CommentsRegex();
    private static readonly Regex DefaultNumbersRx = DefaultNumbersRegex();

    public static async Task<JediName[]> FindJediNamesAsync(Script script) => await script.GetNamesAsync(true, true, true);
    public static JediName[] FindJediNames(Script script) => script.GetNames(true, true, true);

    public static MatchCollection FindKeywords(string text, int startAt = 0) => KeywordsRx.Matches(text, startAt);
    public static MatchCollection FindComments(string text, int startAt = 0) => CommentsRx.Matches(text, startAt);
    public static MatchCollection FindTripleQuotedStrings(string text, int startAt = 0) => DefaultTripleQuotedStringsRx.Matches(text, startAt);
    public static MatchCollection FindSingleQuotedStrings(string text, int startAt = 0) => DefaultSingleQuotedStringsRx.Matches(text, startAt);
    public static MatchCollection FindNumbers(string text, int startAt = 0) => DefaultNumbersRx.Matches(text, startAt);

    [GeneratedRegex(@"('{3}|""{3})[\s\S]*?((?<!\\)|\\\\)\1", RegexOptions.Compiled)]
    private static partial Regex DefaultTripleQuotedStringsRegex();
    [GeneratedRegex(@"('|"").*?((?<!\\)|\\\\)\1", RegexOptions.Compiled)]
    private static partial Regex DefaultSingleQuotedStringsRegex();
    [GeneratedRegex("#.+", RegexOptions.Compiled)]
    private static partial Regex CommentsRegex();
    [GeneratedRegex(@"(\.|\b)\d+([_\.]?\d+)?\2*([eE][+-]?\d+)?(?:(?!\1))?", RegexOptions.Compiled)]
    private static partial Regex DefaultNumbersRegex();

    public static readonly string[] IndentationColorsHexValues = { "#FBB9C5", "#FDD0B1", "#F9EFC7", "#C3EDBF", "#B8DFE6", "#C5BBDE" };
    public static readonly Brush[] IndentationColors = IndentationColorsHexValues.Select(x => x.ToBrush()).ToArray();

    public static readonly string[] BracketColorsHexValues = { "#FBB9C5", "#FDD0B1", "#F9EFC7", "#C3EDBF", "#B8DFE6", "#C5BBDE" };
    public static readonly Brush[] BracketColors = BracketColorsHexValues.Select(x => x.ToBrush()).ToArray();

    public static List<BracketMatch> FindBrackets(string text) {

        List<BracketMatch> matches = new();
        int openBrackets = -1;
        int row = 0;
        int col = 0;

        for (int i = 0; i < text.Length; ++i) {
            char c = text[i];

            if (c == '(' || c == '[' || c == '{') {
                ++openBrackets;
                matches.Add(new(i, row, col, c, openBrackets));
            } else if (c == ')' || c == ']' || c == '}') {
                matches.Add(new(i, row, col, c, openBrackets));
                --openBrackets;
            } else if (c == '\n') {
                col = 0;
                ++row;
            } else
                ++col;
        }

        return matches;
    }

    public static List<IndentMatch> FindIndents(string text) {
        List<IndentMatch> matches = new();
        int col = 0;
        int row = 0;
        int combo = 0;
        bool continueCombo = text.StartsWith(' ');

        for (int i = 0; i < text.Length; ++i) {
            char c = text[i];

            if (c == '\n') {
                col = 0;
                combo = 0;
                continueCombo = true;
                ++row;
            } else if (c == ' ' && continueCombo) {
                ++combo;
                ++col;
            } else {
                continueCombo = false;
                combo = 0;
                ++col;
            }

            if (combo != 0 && combo % 4 == 0)
                matches.Add(new(i - 3, row, col - 4, combo / 4));
        }

        return matches;
    }

    public readonly struct BracketMatch {
        public int Index { get; init; }
        public char BracketChar { get; init; }
        public int Row { get; init; }
        public int Column { get; init; }

        // the amount of open brackets that have not been closed before this bracket
        public int BracketIndex { get; init; }

        public BracketMatch(int index, int row, int col, char bracketChar, int bracketIndex) {
            Index = index;
            BracketChar = bracketChar;
            BracketIndex = bracketIndex;
            Row = row;
            Column = col;
        }
    }

    public readonly struct IndentMatch {
        public int Index { get; init; }
        public int Row { get; init; }
        public int Column { get; init; }
        public int IndentLevel { get; init; }

        public IndentMatch(int index, int row, int col, int indentLevel) {
            Index = index;
            Row = row;
            Column = col;
            IndentLevel = indentLevel;
        }
    }
}