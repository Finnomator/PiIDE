using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PiIDE.Wrapers.JediWraper;
using JediName = PiIDE.Wrapers.JediWraper.ReturnClasses.Name;

namespace PiIDE {

    public static class SyntaxHighlighter {

        // TODO: dont highlight keywords in comments and strings etc.

        private static readonly Regex Rx = new(@$"\b({string.Join('|', Tools.PythonKeywords)})\b", RegexOptions.Compiled);

        public static async Task<JediName[]> FindJediNames(Script script) => await script.GetNames(true, true, true);

        public static Match[] FindKeywords(string text) => Rx.Matches(text).ToArray();
    }
}
