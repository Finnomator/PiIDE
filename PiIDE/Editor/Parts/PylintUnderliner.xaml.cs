using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using PiIDE.Wrapers;

namespace PiIDE.Editor.Parts;

public partial class PylintUnderliner {

    private readonly TextEditor Editor;
    private Size FontSizes => Editor.TextEditorTextBoxCharacterSize;
    private PylintMessage[] CachedMessages = System.Array.Empty<PylintMessage>();

    public PylintUnderliner(TextEditor editor) {
        InitializeComponent();
        Editor = editor;
    }

    public void UpdateUnderline(int upperLineLimit, int lowerLineLimit) => UpdateVisualChildren(CachedMessages, upperLineLimit, lowerLineLimit);

    public void Underline(PylintMessage[] pylintMessages, int upperLineLimit, int lowerLineLimit) {
        CachedMessages = new PylintMessage[pylintMessages.Length];
        pylintMessages.CopyTo(CachedMessages, 0);

        UpdateVisualChildren(CachedMessages, upperLineLimit, lowerLineLimit);
    }

    private void UpdateVisualChildren(PylintMessage[] pylintMessages, int upperLineLimit, int lowerLineLimit) {

        MainGrid.Children.Clear();
        VisualBrush brush = (VisualBrush) Resources["WavyLine"];

        foreach (PylintMessage pylintMessage in pylintMessages) {
            int line = pylintMessage.Line;

            if (line + 1 < upperLineLimit)
                continue;
            else if (line - 1 > lowerLineLimit)
                break;

            int column = pylintMessage.Column;
            int endCol = pylintMessage.EndColumn ?? 1;

            ((Path) brush.Visual).Stroke = PylintMessageColors.MessageTypeToColor(pylintMessage.Type);

            MainGrid.Children.Add(new Line() {
                Stroke = brush,
                StrokeThickness = 3,
                X1 = column * FontSizes.Width + 2,
                X2 = endCol * FontSizes.Width + 2,
                Y1 = line * FontSizes.Height,
                Y2 = line * FontSizes.Height,
            });
        }
    }
}