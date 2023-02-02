using PiIDE.Wrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Point = System.Drawing.Point;

namespace PiIDE {

    public partial class SyntaxHighlighter : UserControl {

        // TODO: dont highlight keywords in comments and strings etc.

        public EventHandler<string>? OnHoverOverWord;
        public EventHandler<string>? OnStoppedHoveringOverWord;
        public EventHandler<string>? OnClickOnWord;

        private readonly Regex Rx = MyRegex();
        private readonly List<Button> OldChildren = new();
        private readonly List<Button> NewChildren = new();
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

                // TODO: This is pretty slow
                Point indexPoint = Tools.GetPointOfIndex(text, startIndex);

                if (indexPoint.Y > lowerLineLimit)
                    break;
                else if (indexPoint.Y < upperLineLimit)
                    continue;

                AddNewButton(keyword, indexPoint, "keyword");
            }
        }

        private void AddNewButton(string keyword, Point indexPoint, string type) {

            // TODO: implement hover effects and stuff

            Button item = new() {
                Content = keyword.Replace("_", "__"),
                Margin = new(indexPoint.X * FontSizes.Width + 2, indexPoint.Y * FontSizes.Height, 0, 0),
                Foreground = TypeColors.TypeToColor(type),
                IsHitTestVisible = false,
                FontFamily = Tools.CascadiaCodeFont,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Transparent,
                BorderThickness = new(0),
                Padding = new(0),
            };
            item.Click += (s, e) => OnClickOnWord?.Invoke(item, "");
            item.MouseEnter += (s, e) => OnHoverOverWord?.Invoke(item, "");
            item.MouseLeave += (s, e) => OnStoppedHoveringOverWord?.Invoke(item, "");
            NewChildren.Add(item);
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

                AddNewButton(name, new Point(col, row - 1), type);
            }
        }

        public IEnumerable<Match> FindKeywords(string text) => Rx.Matches(text).Cast<Match>().Where(match => IsKeyword(match.Value));

        private static bool IsKeyword(string word) => Tools.PythonKeywordsSet.Contains(word);

        [GeneratedRegex("[\\w]+", RegexOptions.Compiled)]
        private static partial Regex MyRegex();
    }
}
