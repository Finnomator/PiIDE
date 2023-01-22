using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PiIDE {

    public partial class SyntaxHighlighter : UserControl {

        private readonly Regex Rx = MyRegex();
        private readonly List<TextBlock> OldChildren = new();
        private readonly List<TextBlock> NewChildren = new();
        private readonly Size FontSizes;

        public SyntaxHighlighter(Size fontSizes) {
            InitializeComponent();
            FontSizes = fontSizes;
        }

        private static int GetColOfIndex(string text, int index) {
            int caretLine = GetRowOfIndex(text, index);

            int offset = 0;
            string[] lines = text.Split("\r\n");

            for (int i = 0; i < caretLine; ++i)
                offset += lines[i].Length + 2;

            return index - offset;
        }

        private static int GetRowOfIndex(string text, int index) {

            int offset = 0;
            string[] lines = text.Split("\r\n");
            int line = 0;

            for (; index >= offset; ++line)
                offset += lines[line].Length + 2;

            return line - 1;
        }

        public async Task HighglightTextAsync(string text, string filePath) {

            NewChildren.Clear();
            AddHighlightedKeywordsToChildren(text);
            await AddHighlightedJediWordsToChildrenAsync(filePath, text);

            if (NewChildren.Count == 0) {
                OldChildren.Clear();
                MainCanvas.Children.Clear();
                return;
            }

            int max = Math.Max(NewChildren.Count, OldChildren.Count);

            for (int i = 0; i < max; ++i) {
                if (i < NewChildren.Count && i < OldChildren.Count && !NewChildren[i].Equals(OldChildren[i])) {
                    MainCanvas.Children.RemoveAt(i);
                    MainCanvas.Children.Insert(i, NewChildren[i]);
                    OldChildren[i] = NewChildren[i];
                } else if (i < NewChildren.Count) {
                    MainCanvas.Children.Add(NewChildren[i]);
                    OldChildren.Add(NewChildren[i]);
                } else if (i < OldChildren.Count) {
                    MainCanvas.Children.RemoveAt(MainCanvas.Children.Count - 1);
                    OldChildren.RemoveAt(OldChildren.Count - 1);
                }
            }
        }

        private void AddHighlightedKeywordsToChildren(string text) {

            foreach (Match match in FindKeywords(text)) {

                int startIndex = match.Index;
                string keyword = match.Value;

                Point indexPoint = new(GetColOfIndex(text, startIndex), GetRowOfIndex(text, startIndex));
                NewChildren.Add(new() {
                    Text = keyword,
                    Margin = new((indexPoint.X + 0.5) * FontSizes.Width - 1.5, indexPoint.Y * FontSizes.Height + 1.5, 0, 0),
                    Foreground = TypeColors.Keyword,
                    //Background = System.Windows.Media.Brushes.White,
                    IsHitTestVisible = false,
                    FontFamily = Tools.CascadiaCodeFont,
                    FontSize = 14,
                });
            }
        }

        private async Task AddHighlightedJediWordsToChildrenAsync(string filePath, string fileContent) {

            JediSyntaxHighlightedWord[] jediSyntaxHighlightedWords = await JediSyntaxHighlighterWraper.GetHighlightedWordsAsync(filePath, fileContent);

            for (int i = 0; i < jediSyntaxHighlightedWords.Length; ++i) {
                JediSyntaxHighlightedWord jediSyntaxHighlightedWord = jediSyntaxHighlightedWords[i];

                int row = jediSyntaxHighlightedWord.Line;
                int col = jediSyntaxHighlightedWord.Column;
                string name = jediSyntaxHighlightedWord.Name;
                string type = jediSyntaxHighlightedWord.Type;

                NewChildren.Add(new() {
                    Text = name,
                    Margin = new((col + 0.5) * FontSizes.Width - 1.5, (row - 1) * FontSizes.Height + 1.5, 0, 0),
                    Foreground = TypeColors.TypeToColorMap[type],
                    //Background = System.Windows.Media.Brushes.White,
                    IsHitTestVisible = false,
                    FontFamily = Tools.CascadiaCodeFont,
                    FontSize = 14,
                });
            }
        }

        public IEnumerable<Match> FindKeywords(string text) => Rx.Matches(text).Cast<Match>().Where(match => IsKeyword(match.Value));

        private static bool IsKeyword(string word) => Tools.PythonKeywordsSet.Contains(word);

        [GeneratedRegex("[\\w]+", RegexOptions.Compiled)]
        private static partial Regex MyRegex();
    }
}
