using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Editor.Parts {
    public class TextBoxWithDrawingGroup : TextBox {
        private readonly DrawingGroup DrawingGroup = new();

        private readonly List<Action<DrawingContext>> RenderActions = new();
        private readonly List<Func<DrawingContext, Task>> AsyncRenderActions = new();

        public Action<DrawingContext> DefaultRenderAction { get; init; }
        public FormattedText TextAsFormattedText { get; private set; }
        private bool IsBusy;
        private bool GotNewerRequest;

        // TODO: Make it work with backgrounds

        public TextBoxWithDrawingGroup() {
            Foreground = null;
            Background = null;
            CaretBrush = Brushes.White;

            DefaultRenderAction = (dc) => {
                TextAsFormattedText.SetForegroundBrush(CaretBrush);
            };

            TextAsFormattedText = GetTextAsFormattedText();

            //AddRenderAction(DefaultRenderAction);

            TextChanged += (s, e) => {
                TextAsFormattedText = GetTextAsFormattedText();
                Render();
            };
        }

        private FormattedText GetTextAsFormattedText() {
            return new(
                textToFormat: Text,
                culture: CultureInfo.GetCultureInfo("en-us"),
                flowDirection: FlowDirection.LeftToRight,
                typeface: new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                emSize: base.FontSize,
                foreground: Brushes.White,
                pixelsPerDip: VisualTreeHelper.GetDpi(this).PixelsPerDip
            );
        }

        public void AddAsyncRenderAction(Func<DrawingContext, Task> action) => AsyncRenderActions.Add(action);

        public void AddRenderAction(Action<DrawingContext> action) => RenderActions.Add(action);

        public void RemoveRenderAction(Action<DrawingContext> action) => RenderActions.Remove(action);

        private async void Render() {

            if (IsBusy) {
                GotNewerRequest = true;
                return;
            }

            IsBusy = true;

            using (DrawingContext dc = DrawingGroup.Open()) {
                foreach (Action<DrawingContext> action in RenderActions)
                    action(dc);
                foreach (Func<DrawingContext, Task> func in AsyncRenderActions)
                    await func(dc);
                dc.DrawText(TextAsFormattedText, new(2, 0));
            }

            IsBusy = false;

            if (GotNewerRequest) {
                Render();
                GotNewerRequest = false;
            }
        }

        protected override void OnRender(DrawingContext drawingContext) {
            Render();
            drawingContext.DrawDrawing(DrawingGroup);
        }
    }
}
