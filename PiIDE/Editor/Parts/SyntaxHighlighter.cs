using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using static PiIDE.Wrapers.JediWraper;
using JediName = PiIDE.Wrapers.JediWraper.ReturnClasses.Name;
using Point = System.Drawing.Point;

namespace PiIDE {

    public static partial class SyntaxHighlighter {

        private static readonly Regex KeywordsRx = new(@$"\b({string.Join('|', Tools.PythonKeywords)})\b", RegexOptions.Compiled);
        private static readonly Regex StringsRx = StringsRegex();
        private static readonly Regex CommentsRx = CommentsRegex();
        private static readonly Regex NumbersRx = NumbersRegex();

        public static async Task<JediName[]> FindJediNamesAsync(Script script) => await script.GetNamesAsync(true, true, true);
        public static JediName[] FindJediNames(Script script) => script.GetNames(true, true, true);

        public static Match[] FindKeywords(string text) => KeywordsRx.Matches(text).ToArray();
        public static Match[] FindComments(string text) => CommentsRx.Matches(text).ToArray();
        public static Match[] FindStrings(string text) => StringsRx.Matches(text).ToArray();
        public static Match[] FindNumbers(string text) => NumbersRx.Matches(text).ToArray();

        [GeneratedRegex("(['\"])(.*?)\\1", RegexOptions.Compiled)]
        private static partial Regex StringsRegex();
        [GeneratedRegex("#.+", RegexOptions.Compiled)]
        private static partial Regex CommentsRegex();
        [GeneratedRegex("\\d+(\\.\\d+)?(e[-+]?\\d+)?", RegexOptions.Compiled)]
        private static partial Regex NumbersRegex();

        public static readonly string[] BracketColorsHexValues = new string[] { "#FBB9C5", "#FDD0B1", "#F9EFC7", "#C3EDBF", "#B8DFE6", "#C5BBDE" };
        public static readonly Color[] BracketColors = BracketColorsHexValues.Select(x => x.ToColor()).ToArray();

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
    }
}
