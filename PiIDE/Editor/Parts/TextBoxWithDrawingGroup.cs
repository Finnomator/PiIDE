using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Editor.Parts {
    public class TextBoxWithDrawingGroup : TextBox {
        private readonly DrawingGroup DrawingGroup = new();

        private readonly HashSet<Action<DrawingContext>> RenderActions = new();

        public Action<DrawingContext> DefaultRenderAction { get; init; }
        public FormattedText VisibleTextAsFormattedText { get; private set; }

        public TextEditor? TextEditor { get; set; }
        int i;

        // TODO: Make it work with backgrounds

        public TextBoxWithDrawingGroup() {
            Foreground = null;
            Background = null;
            CaretBrush = Brushes.White;

            DefaultRenderAction = (dc) => {
                VisibleTextAsFormattedText!.SetForegroundBrush(CaretBrush);
            };

            VisibleTextAsFormattedText = GetVisibleTextAsFormattedText();

            AddRenderAction(DefaultRenderAction);

            SizeChanged += (s, e) => Render();

            TextChanged += (s, e) => Render();

            Loaded += delegate {
                if (TextEditor == null)
                    return;
                TextEditor.MainScrollViewer.ScrollChanged += (s, e) => {
                    if (e.VerticalChange != 0)
                        Render();
                };

                TextEditor.MainScrollViewer.SizeChanged += (s, e) => Render();
            };
        }

        private FormattedText GetVisibleTextAsFormattedText() => TextEditor == null ? GetTextAsFormattedText("") : GetTextAsFormattedText(TextEditor.VisibleText);

        private FormattedText GetTextAsFormattedText(string text) => new(
                textToFormat: text,
                culture: CultureInfo.GetCultureInfo("en-us"),
                flowDirection: FlowDirection.LeftToRight,
                typeface: new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                emSize: FontSize,
                foreground: Brushes.White,
                pixelsPerDip: VisualTreeHelper.GetDpi(this).PixelsPerDip
            );

        public void AddRenderAction(Action<DrawingContext> action) => RenderActions.Add(action);

        public void RemoveRenderAction(Action<DrawingContext> action) => RenderActions.Remove(action);

        public void Render() {
            Debug.WriteLine("Render " + i++);
            VisibleTextAsFormattedText = GetVisibleTextAsFormattedText();
            using DrawingContext dc = DrawingGroup.Open();
            foreach (Action<DrawingContext> action in RenderActions)
                action(dc);
            dc.DrawText(VisibleTextAsFormattedText, new(2, TextEditor == null ? 0 : TextEditor.FirstVisibleLineNum * TextEditor.TextEditorTextBoxCharacterSize.Height));
        }

        protected override void OnRender(DrawingContext drawingContext) {
            Render();
            drawingContext.DrawDrawing(DrawingGroup);
        }
    }
}
