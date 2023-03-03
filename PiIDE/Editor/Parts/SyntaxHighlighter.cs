using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PiIDE.Wrapers.JediWraper;
using JediName = PiIDE.Wrapers.JediWraper.ReturnClasses.Name;

namespace PiIDE {

    public static partial class SyntaxHighlighter {

        private static readonly Regex KeywordsRx = new(@$"\b({string.Join('|', Tools.PythonKeywords)})\b", RegexOptions.Compiled);
        private static readonly Regex StringsRx = StringsRegex();
        private static readonly Regex CommentsRx = CommentsRegex();
        private static readonly Regex NumbersRx = NumbersRegex();

        public static async Task<JediName[]> FindJediNames(Script script) => await script.GetNames(true, true, true);

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
    }
}
