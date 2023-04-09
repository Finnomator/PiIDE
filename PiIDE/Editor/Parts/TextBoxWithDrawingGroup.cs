using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Editor.Parts;

public class TextBoxWithDrawingGroup : TextBox {
    private readonly DrawingGroup DrawingGroup = new();
    private readonly SortedList<int, Action<DrawingContext>> RenderActions = new();
    private Typeface? CachedTypeface;
    private readonly double CachedPixelsPerDip;

    public Action<DrawingContext> DefaultRenderAction { get; }
    public FormattedText? VisibleTextAsFormattedText { get; private set; }

    public TextEditor TextEditor { get; set; } = null!;

    private readonly Stopwatch Sw = new();

    private ScrollChangedEventArgs? OldScrollChangedEventArgs;

    // TODO: Make it work with backgrounds

    public TextBoxWithDrawingGroup() {
        Foreground = null;
        Background = null;
        CaretBrush = Brushes.White;

        CachedPixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        DefaultRenderAction = _ => VisibleTextAsFormattedText!.SetForegroundBrush(CaretBrush);

        AddRenderAction(0, DefaultRenderAction);

        TextChanged += TextBoxWithDrawingGroup_TextChanged;
        Loaded += TextBoxWithDrawingGroup_Loaded;
    }

    private void TextBoxWithDrawingGroup_TextChanged(object sender, TextChangedEventArgs e) => Render();

    private void TextBoxWithDrawingGroup_Loaded(object sender, RoutedEventArgs _) {

        Debug.Assert(TextEditor != null);

        TextEditor.MainScrollViewer.SizeChanged += (_, _) => Render();

        TextEditor.MainScrollViewer.ScrollChanged += (_, e) => {
            if (e.VerticalChange != 0 && e != OldScrollChangedEventArgs)
                Render();
            OldScrollChangedEventArgs = e;
        };
    }

    private void SetVisibleTextAsFormattedText() => VisibleTextAsFormattedText = GetTextAsFormattedText(TextEditor.GetVisibleText());

    private FormattedText GetTextAsFormattedText(string text) {

        CachedTypeface ??= new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

        return new(
            textToFormat: text,
            culture: CultureInfo.GetCultureInfo("en-us"),
            flowDirection: 0,
            typeface: CachedTypeface,
            emSize: FontSize,
            foreground: Brushes.White,
            pixelsPerDip: CachedPixelsPerDip
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

            Sw.Restart();

            SetVisibleTextAsFormattedText();
            using (DrawingContext dc1 = DrawingGroup.Open()) {
                foreach (Action<DrawingContext> action in RenderActions.Values)
                    action(dc1);
                dc1.DrawText(VisibleTextAsFormattedText, new(2, TextEditor.GetFirstVisibleLineNum() * TextEditor.TextEditorTextBoxCharacterSize.Height));
            }

            Sw.Stop();

            Tools.StatsWindow!.AddRenderStat(Sw.ElapsedMilliseconds);

            return;
        }

        SetVisibleTextAsFormattedText();
        using DrawingContext dc = DrawingGroup.Open();
        foreach (Action<DrawingContext> action in RenderActions.Values)
            action(dc);
        dc.DrawText(VisibleTextAsFormattedText, new(2, TextEditor.GetFirstVisibleLineNum() * TextEditor.TextEditorTextBoxCharacterSize.Height));
    }

    protected override void OnRender(DrawingContext drawingContext) {
        Render();
        drawingContext.DrawDrawing(DrawingGroup);
    }
}