using PiIDE.Wrapers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PiIDE {

    public partial class PylingUnderliner : UserControl {

        private readonly TextEditor Editor;
        private Size FontSizes => Editor.TextEditorTextBoxCharacterSize;
        private readonly VisualBrush WavyLine;
        private PylintMessage[] CachedMessages = System.Array.Empty<PylintMessage>();

        public PylingUnderliner(TextEditor editor) {
            InitializeComponent();
            Editor = editor;

            WavyLine = new() {
                Viewbox = new Rect(0, 0, 3, 2),
                Viewport = new Rect(0, 1, 4, 5),
                ViewboxUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.Tile,
                Visual = new Path() {
                    Data = Geometry.Parse("M 0,1 C1,1 1,2 3,1"),
                    Stroke = Brushes.Red,
                    StrokeThickness = 0.2,
                    StrokeEndLineCap = PenLineCap.Square,
                    StrokeStartLineCap = PenLineCap.Square,
                }
            };
        }

        public void UpdateUnderline(int upperLineLimit, int lowerLineLimit) => UpdateVisualChildren(CachedMessages, upperLineLimit, lowerLineLimit);

        public void Underline(PylintMessage[] pylintMessages, int upperLineLimit, int lowerLineLimit) {
            CachedMessages = new PylintMessage[pylintMessages.Length];
            pylintMessages.CopyTo(CachedMessages, 0);

            UpdateVisualChildren(CachedMessages, upperLineLimit, lowerLineLimit);
        }

        private void UpdateVisualChildren(PylintMessage[] pylintMessages, int upperLineLimit, int lowerLineLimit) {

            MainGrid.Children.Clear();
            VisualBrush brush = (VisualBrush) Resources["asdf"];

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
}
