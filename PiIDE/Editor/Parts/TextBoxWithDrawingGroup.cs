﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Editor.Parts {
    public class TextBoxWithDrawingGroup : TextBox {
        private readonly DrawingGroup DrawingGroup = new();
        private readonly SortedList<int, Action<DrawingContext>> RenderActions = new();
        private Typeface cachedTypeface;
        private readonly double cachedPixelsPerDip;

        public Action<DrawingContext> DefaultRenderAction { get; init; }
        public FormattedText VisibleTextAsFormattedText { get; private set; }

        public TextEditor TextEditor { get; set; }

        private readonly Stopwatch sw = new();

        // TODO: Make it work with backgrounds

        public TextBoxWithDrawingGroup() {
            Foreground = null;
            Background = null;
            CaretBrush = Brushes.White;

            cachedPixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            DefaultRenderAction = (dc) => VisibleTextAsFormattedText!.SetForegroundBrush(CaretBrush);

            AddRenderAction(0, DefaultRenderAction);

            TextChanged += TextBoxWithDrawingGroup_TextChanged;
            Loaded += TextBoxWithDrawingGroup_Loaded;
        }

        private void TextBoxWithDrawingGroup_TextChanged(object sender, TextChangedEventArgs e) => Render();

        private void TextBoxWithDrawingGroup_Loaded(object sender, RoutedEventArgs e) {

            Debug.Assert(TextEditor != null);

            TextEditor.MainScrollViewer.SizeChanged += (s, e) => Render();

            TextEditor.MainScrollViewer.ScrollChanged += (s, e) => {
                if (e.VerticalChange != 0)
                    Render();
            };
        }

        private void SetVisibleTextAsFormattedText() => VisibleTextAsFormattedText = GetTextAsFormattedText(TextEditor.VisibleText);

        private FormattedText GetTextAsFormattedText(string text) {

            cachedTypeface ??= new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

            return new(
                textToFormat: text,
                culture: CultureInfo.GetCultureInfo("en-us"),
                flowDirection: 0,
                typeface: cachedTypeface,
                emSize: FontSize,
                foreground: Brushes.White,
                pixelsPerDip: cachedPixelsPerDip
            );
        }

        public void AddRenderAction(int priority, Action<DrawingContext> action) {
            if (!RenderActions.ContainsValue(action))
                RenderActions.Add(priority, action);
        }

        public void RemoveRenderAction(Action<DrawingContext> action) {
            if (RenderActions.ContainsValue(action))
                RenderActions.Remove(RenderActions.IndexOfValue(action));
        }

        public void Render() {

            if (Tools.UpdateStats) {

                sw.Restart();

                SetVisibleTextAsFormattedText();
                using (DrawingContext dc1 = DrawingGroup.Open()) {
                    foreach (Action<DrawingContext> action in RenderActions.Values)
                        action(dc1);
                    dc1.DrawText(VisibleTextAsFormattedText, new(2, TextEditor.FirstVisibleLineNum * TextEditor.TextEditorTextBoxCharacterSize.Height));
                }

                sw.Stop();

                Tools.StatsWindow!.AddRenderStat(sw.ElapsedMilliseconds);

                return;
            }

            SetVisibleTextAsFormattedText();
            using DrawingContext dc = DrawingGroup.Open();
            foreach (Action<DrawingContext> action in RenderActions.Values)
                action(dc);
            dc.DrawText(VisibleTextAsFormattedText, new(2, TextEditor.FirstVisibleLineNum * TextEditor.TextEditorTextBoxCharacterSize.Height));
        }

        protected override void OnRender(DrawingContext drawingContext) {
            Render();
            drawingContext.DrawDrawing(DrawingGroup);
        }
    }
}
