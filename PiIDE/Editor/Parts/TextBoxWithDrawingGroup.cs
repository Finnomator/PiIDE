using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Editor.Parts {
    public class TextBoxWithDrawingGroup : TextBox {
        private readonly DrawingGroup DrawingGroup = new();

        public event EventHandler? StartedRender;
        public event EventHandler? FinishedRender;

        private readonly List<Action<DrawingContext>> RenderActions = new();
        private readonly List<Func<DrawingContext, Task>> AsyncRenderActions = new();

        public Action<DrawingContext> DefaultRenderAction { get; init; }
        public FormattedText VisibleTextAsFormattedText { get; private set; }

        public TextEditor? TextEditor { get; set; }

        // TODO: Make it work with backgrounds

        public TextBoxWithDrawingGroup() {
            Foreground = null;
            Background = null;
            CaretBrush = Brushes.White;

            DefaultRenderAction = (dc) => {
                VisibleTextAsFormattedText.SetForegroundBrush(CaretBrush);
            };

            VisibleTextAsFormattedText = GetVisibleTextAsFormattedText();

            AddRenderAction(DefaultRenderAction);

            TextChanged += (s, e) => {
                VisibleTextAsFormattedText = GetVisibleTextAsFormattedText();
                Render();
            };

            Loaded += delegate {
                if (TextEditor is null)
                    return;
                TextEditor.MainScrollViewer.ScrollChanged += (s, e) => {
                    VisibleTextAsFormattedText = GetVisibleTextAsFormattedText();
                    Render();
                };
            };
        }

        private FormattedText GetVisibleTextAsFormattedText() => TextEditor is null ? GetTextAsFormattedText("") : GetTextAsFormattedText(TextEditor.VisibleText);

        private FormattedText GetTextAsFormattedText(string text) {
            return new(
                textToFormat: text,
                culture: CultureInfo.GetCultureInfo("en-us"),
                flowDirection: FlowDirection.LeftToRight,
                typeface: new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                emSize: FontSize,
                foreground: Brushes.White,
                pixelsPerDip: VisualTreeHelper.GetDpi(this).PixelsPerDip
            );
        }

        public void AddAsyncRenderAction(Func<DrawingContext, Task> action) => AsyncRenderActions.Add(action);

        public void AddRenderAction(Action<DrawingContext> action) => RenderActions.Add(action);

        public void RemoveRenderAction(Action<DrawingContext> action) => RenderActions.Remove(action);

        public void Render() {

            StartedRender?.Invoke(this, EventArgs.Empty);

            using (DrawingContext dc = DrawingGroup.Open()) {
                foreach (Action<DrawingContext> action in RenderActions)
                    action(dc);
                dc.DrawText(VisibleTextAsFormattedText, new(2, TextEditor is null? 0 : TextEditor.FirstVisibleLineNum * TextEditor.TextEditorTextBoxCharacterSize.Height));
            }

            FinishedRender?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnRender(DrawingContext drawingContext) {
            Render();
            drawingContext.DrawDrawing(DrawingGroup);
        }
    }
}
