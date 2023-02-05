using PiIDE.Wrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Point = System.Drawing.Point;

namespace PiIDE {

    public partial class SyntaxHighlighter : UserControl {

        // TODO: dont highlight keywords in comments and strings etc.

        public EventHandler<JediName>? OnHoverOverWord;
        public EventHandler<JediName>? OnStoppedHoveringOverWord;
        public EventHandler<JediName>? OnClickOnWord;

        private readonly Regex Rx = MyRegex();
        private readonly List<Button> OldChildren = new();
        private readonly List<Button> NewChildren = new();
        private readonly Size FontSizes;

        public SyntaxHighlighter(Size fontSizes) {
            InitializeComponent();
            FontSizes = fontSizes;
        }

        public void HighglightText(string text, string filePath, bool enableTypeHints, int startLine, int endLine) {

            NewChildren.Clear();
            AddHighlightedKeywordsToChildren(text, startLine, endLine);
            AddHighlightedJediWordsToChildren(filePath, text, enableTypeHints, startLine, endLine);

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

                Point indexPoint = Tools.GetPointOfIndex(text, startIndex);

                if (indexPoint.Y > lowerLineLimit)
                    break;
                else if (indexPoint.Y < upperLineLimit)
                    continue;

                NewChildren.Add(CreateKeywordButton(match.Value, indexPoint));
            }
        }

        private Button CreateKeywordButton(string keyword, Point indexPoint) {
            return new() {
                Content = keyword,
                Margin = new(indexPoint.X * FontSizes.Width + 2, indexPoint.Y * FontSizes.Height, 0, 0),
                Foreground = TypeColors.TypeToColor("keyword"),
                Background = Brushes.Transparent,
                IsHitTestVisible = false,
                FontFamily = Tools.CascadiaCodeFont,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = 14,
                BorderThickness = new(0),
                Padding = new(0),
            };
        }

        private Button CreateJediNameButton(JediName jediName, Point indexPoint) {

            // TODO: implement hover effects and stuff

            Button item = new() {
                Content = jediName.Name.Replace("_", "__"),
                Margin = new(indexPoint.X * FontSizes.Width + 2, indexPoint.Y * FontSizes.Height, 0, 0),
                Foreground = TypeColors.TypeToColor(jediName.Type),
                IsHitTestVisible = true,
                Cursor = Cursors.Hand,
                FontFamily = Tools.CascadiaCodeFont,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                // Background = Brushes.Transparent,
                BorderThickness = new(0),
                Padding = new(0),
            };

            if (jediName.Type != "keyword") {
                item.Click += (s, e) => {
                    OnClickOnWord?.Invoke(item, jediName);
                };
                item.MouseEnter += (s, e) => {
                    OnHoverOverWord?.Invoke(item, jediName);
                };
                item.MouseLeave += (s, e) => {
                    OnStoppedHoveringOverWord?.Invoke(item, jediName);
                };
            }

            return item;
        }

        private void AddHighlightedJediWordsToChildren(string filePath, string fileContent, bool enableTypeHints, int upperLineLimit, int lowerLineLimit) {

            JediName[] jediSyntaxHighlightedWords = JediSyntaxHighlighterWraper.GetHighlightedWords(filePath, fileContent, enableTypeHints);

            for (int i = 0; i < jediSyntaxHighlightedWords.Length; ++i) {
                JediName jediSyntaxHighlightedWord = jediSyntaxHighlightedWords[i];

                int? row = jediSyntaxHighlightedWord.Line;
                int? col = jediSyntaxHighlightedWord.Column;

                if (col is null || row is null)
                    throw new NullReferenceException();

                if (row > lowerLineLimit)
                    break;
                else if (row < upperLineLimit)
                    continue;

                NewChildren.Add(CreateJediNameButton(jediSyntaxHighlightedWords[i], new Point((int) col, (int) (row - 1))));
            }
        }

        public IEnumerable<Match> FindKeywords(string text) => Rx.Matches(text).Cast<Match>().Where(match => IsKeyword(match.Value));

        private static bool IsKeyword(string word) => Tools.PythonKeywordsSet.Contains(word);

        [GeneratedRegex("[\\w]+", RegexOptions.Compiled)]
        private static partial Regex MyRegex();
    }
}
