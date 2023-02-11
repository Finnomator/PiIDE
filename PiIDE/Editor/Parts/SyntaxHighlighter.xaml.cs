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
using JediName = PiIDE.Wrapers.JediWraper.ReturnClasses.Name;
using WraperRepl = PiIDE.Wrapers.JediWraper.WraperRepl;
using static PiIDE.Wrapers.JediWraper;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

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
        private readonly string FilePath;
        private JediName[]? CachedJediNames;
        private Match[]? CachedKeywordMatches;
        private string? CachedKeywordText;

        public SyntaxHighlighter(Size fontSizes, string filePath) {
            InitializeComponent();
            FontSizes = fontSizes;
            FilePath = filePath;
        }

        public void ForceAllButtonsToStayEnabled(bool enabled) {
            for (int i = 0; i < MainCanvas.Children.Count; ++i) {
                HighlighterButton button = (HighlighterButton) MainCanvas.Children[i];
                button.StayEnabled = enabled;
            }
        }

        public void UpdateHighlighting(int startLine, int endLine) {

            if (CachedJediNames is null || CachedKeywordText is null)
                return;

            NewChildren.Clear();
            AddJediNamesToChildren(CachedJediNames, startLine, endLine);
            AddHighlightedKeywordsToChildren(CachedKeywordText, startLine, endLine);
            UpdateVisualChildren();
        }

        public async Task HighglightTextAsync(string text, int startLine, int endLine) {

            NewChildren.Clear();
            Script script = new(text, FilePath);
            AddHighlightedKeywordsToChildren(text, startLine, endLine);
            await AddHighlightedJediWordsToChildrenAsync(script, startLine, endLine);

            if (NewChildren.Count == 0) {
                OldChildren.Clear();
                MainCanvas.Children.Clear();
                return;
            }

            UpdateVisualChildren();
        }

        private void UpdateVisualChildren() {
            int min = Math.Min(NewChildren.Count, OldChildren.Count);

            for (int i = 0; i < min; ++i) {
                if (NewChildren[i] != OldChildren[i]) {
                    MainCanvas.Children.RemoveAt(i);
                    MainCanvas.Children.Insert(i, NewChildren[i]);
                    OldChildren[i] = NewChildren[i];
                }
            }

            for (int i = min; i < NewChildren.Count; ++i) {
                MainCanvas.Children.Add(NewChildren[i]);
                OldChildren.Add(NewChildren[i]);
            }

            for (int i = min; i < OldChildren.Count; ++i) {
                MainCanvas.Children.RemoveAt(i);
                OldChildren.RemoveAt(i);
            }
        }


        private void AddHighlightedKeywordsToChildren(string text, int upperLineLimit, int lowerLineLimit) {
            CachedKeywordMatches = FindKeywords(text);
            CachedKeywordText = text;
            AddKeywordsToChildren(CachedKeywordMatches, text, upperLineLimit, lowerLineLimit);
        }

        private void AddKeywordsToChildren(Match[] keywordMatches, string text, int upperLineLimit, int lowerLineLimit) {
            for (int i = 0; i < keywordMatches.Length; i++) {

                Match match = keywordMatches[i];

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
                BorderThickness = Tools.ZeroThichness,
                Padding = Tools.ZeroThichness,
            };
        }

        private HighlighterButton CreateJediNameButton(JediName jediName, Point indexPoint) {

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
                BorderThickness = Tools.ZeroThichness,
                Padding = Tools.ZeroThichness,
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

        private async Task AddHighlightedJediWordsToChildrenAsync(Script script, int upperLineLimit, int lowerLineLimit) {
            CachedJediNames = await script.GetNames(true, true, true);
            AddJediNamesToChildren(CachedJediNames, upperLineLimit, lowerLineLimit);
        }

        private void AddJediNamesToChildren(JediName[] jediNames, int upperLineLimit, int lowerLineLimit) {
            for (int i = 0; i < jediNames.Length; ++i) {
                JediName jediName = jediNames[i];

                if (jediName.Type == "keyword")
                    continue;

                int? row = jediName.Line;
                int? col = jediName.Column;

                if (col is null || row is null)
                    throw new NullReferenceException();

                if (row > lowerLineLimit)
                    break;
                else if (row < upperLineLimit)
                    continue;

                NewChildren.Add(CreateJediNameButton(jediName, new Point((int) col, (int) (row - 1))));
            }
        }

        public Match[] FindKeywords(string text) => Rx.Matches(text).Cast<Match>().Where(match => IsKeyword(match.Value)).ToArray();

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

            protected override async void OnMouseEnter(MouseEventArgs e) {
                IsHitTestVisible = StayEnabled;

                await Task.Delay(500);
                System.Windows.Point mousePos = Mouse.GetPosition(this);

                if (IsMouseOver()) {
                    MouseEnter?.Invoke(this, e);
                    WaitForMouseLeave();
                } else {
                    IsHitTestVisible = true;
                }
            }

            private async void WaitForMouseLeave() {

                while (IsMouseOver())
                    await Task.Delay(100);

                MouseLeave?.Invoke(this, EventArgs.Empty);
                IsHitTestVisible = true;
            }

            private new bool IsMouseOver() {
                System.Windows.Point mousePos = Mouse.GetPosition(this);
                // The point.X is negative if the mouse comes from the right of the button, for some reason...
                mousePos.X = Math.Abs(mousePos.X);
                return _rect.Contains(mousePos);
            }
        }
    }
}
