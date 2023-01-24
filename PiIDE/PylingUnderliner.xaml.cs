using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PiIDE {

    public partial class PylingUnderliner : UserControl {

        private readonly Size FontSizes;
        private readonly VisualBrush WavyLine;

        public PylingUnderliner(Size fontSizes) {
            InitializeComponent();
            FontSizes = fontSizes;

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

        public void Underline(PylintMessage[] pylintMessages, int upperLineLimit, int lowerLineLimit) {

            MainGrid.Children.Clear();

            for (int i = 0; i < pylintMessages.Length; ++i) {
                PylintMessage pylintMessage = pylintMessages[i];

                int line = pylintMessage.Line;

                if (line - 1 > lowerLineLimit || line + 1 < upperLineLimit)
                    continue;

                int column = pylintMessage.Column;
                int endCol = pylintMessage.EndColumn is null ? 1 : (int) pylintMessage.EndColumn;



                MainGrid.Children.Add(new Line() {
                    //Stroke = WavyLine,
                    StrokeThickness = 2,
                    X1 = column * FontSizes.Width + 1,
                    X2 = endCol * FontSizes.Width + 1,
                    Y1 = line * FontSizes.Height,
                    Y2 = line * FontSizes.Height,
                    Stroke = Brushes.Red,
                });
            }
        }
    }
}
