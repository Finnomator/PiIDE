using PiIDE.Wrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private readonly List<HighlighterButton> OldChildren = new();
        private readonly List<HighlighterButton> NewChildren = new();
        private readonly Size FontSizes;

        public SyntaxHighlighter(Size fontSizes) {
            InitializeComponent();
            FontSizes = fontSizes;
        }

        public void ForceAllButtonsToStayEnabled(bool enabled) {
            for (int i = 0; i < MainCanvas.Children.Count; ++i) {
                HighlighterButton button = (HighlighterButton) MainCanvas.Children[i];
                button.StayEnabled = enabled;
            }
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


            // TODO: There is some bug here
            for (int i = 0; i < max; ++i) {
                if (i < NewChildren.Count && i < OldChildren.Count) {
                    if (NewChildren[i] != OldChildren[i]) {
                        MainCanvas.Children.RemoveAt(i);
                        MainCanvas.Children.Insert(i, NewChildren[i]);
                        OldChildren[i] = NewChildren[i];
                    }
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

        private HighlighterButton CreateKeywordButton(string keyword, Point indexPoint) {
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

        private HighlighterButton CreateJediNameButton(JediName jediName, Point indexPoint) {

            // TODO: implement hover effects and stuff

            HighlighterButton item = new() {
                Content = jediName.Name.Replace("_", "__"),
                Margin = new(indexPoint.X * FontSizes.Width + 2, indexPoint.Y * FontSizes.Height, 0, 0),
                Foreground = TypeColors.TypeToColor(jediName.Type),
                Cursor = Cursors.Hand,
                FontFamily = Tools.CascadiaCodeFont,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Transparent,
                BorderThickness = new(0),
                Padding = new(0),
            };

            item.Click += (s, e) => {
                OnClickOnWord?.Invoke(item, jediName);
            };
            item.MouseEnter += (s, e) => {
                OnHoverOverWord?.Invoke(item, jediName);
            };
            item.MouseLeave += (s, e) => {
                OnStoppedHoveringOverWord?.Invoke(item, jediName);
            };

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


        private class HighlighterButton : Button {

            public new event EventHandler? MouseLeave;
            public new event MouseEventHandler? MouseEnter;

            private bool _stayEnabled;
            public bool StayEnabled {
                get => _stayEnabled; set {
                    _stayEnabled = value;
                    IsHitTestVisible = value;
                }
            }

            private Rect _rect;

            public HighlighterButton() {
                Loaded += delegate {
                    _rect = new(0, 0, ActualWidth, ActualHeight);
                };
            }

            public static bool operator ==(HighlighterButton b1, HighlighterButton b2) {
                if (b1 is null)
                    return b2 is null;
                return (string) b1.Content == (string) b2.Content && b1.Margin == b2.Margin;
            }

            public static bool operator !=(HighlighterButton b1, HighlighterButton b2) => !(b1 == b2);

            protected override void OnMouseEnter(MouseEventArgs e) {
                MouseEnter?.Invoke(this, e);
                IsHitTestVisible = StayEnabled;
                WaitForMouseLeave();
            }

            private async void WaitForMouseLeave() {
                System.Windows.Point mousePos = Mouse.GetPosition(this);

                // The point.X is negative if the mouse comes from the right of the button, for some reason...
                mousePos = new(Math.Abs(mousePos.X), mousePos.Y);

                while (_rect.Contains(mousePos)) {
                    await Task.Delay(100);
                    mousePos = Mouse.GetPosition(this);
                }

                MouseLeave?.Invoke(this, EventArgs.Empty);
                IsHitTestVisible = true;
            }
        }
    }
}
