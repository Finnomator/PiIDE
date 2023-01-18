using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PiIDE {
    public static class Tools {
        public static readonly BrushConverter Converter = new();
        public static readonly Brush SelectedBrush = (Brush) Converter.ConvertFromString("#14000000");
        public static readonly Brush UnselectedBrush = Brushes.Transparent;
    }
}
