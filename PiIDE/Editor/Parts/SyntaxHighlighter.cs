using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PiIDE.Wrapers.JediWraper;
using JediName = PiIDE.Wrapers.JediWraper.ReturnClasses.Name;

namespace PiIDE {

    public static class SyntaxHighlighter {

        private static readonly Regex KeywordsRx = new(@$"\b({string.Join('|', Tools.PythonKeywords)})\b", RegexOptions.Compiled);
        private static readonly Regex StringsRx = new(@"(['""])(.*?)\1", RegexOptions.Compiled);
        private static readonly Regex CommentsRx = new("#.+", RegexOptions.Compiled);
        private static readonly Regex NumbersRx = new(@"\d+(\.\d+)?(e[-+]?\d+)?", RegexOptions.Compiled);

        public static async Task<JediName[]> FindJediNames(Script script) => await script.GetNames(true, true, true);

        public static Match[] FindKeywords(string text) => KeywordsRx.Matches(text).ToArray();
        public static Match[] FindComments(string text) => CommentsRx.Matches(text).ToArray();
        public static Match[] FindStrings(string text) => StringsRx.Matches(text).ToArray();
        public static Match[] FindNumbers(string text) => NumbersRx.Matches(text).ToArray();
    }
}
