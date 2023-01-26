using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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



        public void HighglightText(string text, string filePath, int startLine, int endLine) {

            NewChildren.Clear();
            AddHighlightedKeywordsToChildren(text, startLine, endLine);
            AddHighlightedJediWordsToChildren(filePath, text, startLine, endLine);

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

        private void AddHighlightedKeywordsToChildren(string text, int upperLineLimit, int lowerLineLimit) {

            foreach (Match match in FindKeywords(text)) {

                int startIndex = match.Index;
                string keyword = match.Value;

                Point indexPoint = new(Tools.GetColOfIndex(text, startIndex), Tools.GetRowOfIndex(text, startIndex));

                if (indexPoint.Y > lowerLineLimit)
                    break;
                else if (indexPoint.Y < upperLineLimit)
                    continue;

                NewChildren.Add(new() {
                    Text = keyword,
                    Margin = new(indexPoint.X * FontSizes.Width + 2, indexPoint.Y * FontSizes.Height + 0.3, 0, 0),
                    Foreground = TypeColors.Keyword,
                    //Background = System.Windows.Media.Brushes.White,
                    IsHitTestVisible = false,
                    FontFamily = Tools.CascadiaCodeFont,
                    FontSize = 14,
                });
            }
        }

        private void AddHighlightedJediWordsToChildren(string filePath, string fileContent, int upperLineLimit, int lowerLineLimit) {

            JediSyntaxHighlightedWord[] jediSyntaxHighlightedWords = JediSyntaxHighlighterWraper.GetHighlightedWords(filePath, fileContent);

            for (int i = 0; i < jediSyntaxHighlightedWords.Length; ++i) {
                JediSyntaxHighlightedWord jediSyntaxHighlightedWord = jediSyntaxHighlightedWords[i];

                int row = jediSyntaxHighlightedWord.Line;
                int col = jediSyntaxHighlightedWord.Column;
                string name = jediSyntaxHighlightedWord.Name;
                string type = jediSyntaxHighlightedWord.Type;

                if (row > lowerLineLimit)
                    break;
                else if (row < upperLineLimit)
                    continue;

                NewChildren.Add(new() {
                    Text = name,
                    Margin = new(col * FontSizes.Width + 2, (row - 1) * FontSizes.Height + 0.3, 0, 0),
                    Foreground = TypeColors.TypeToColor(type),
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
