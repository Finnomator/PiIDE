using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PiIDE.Editor.Parts {
    public class BoardEditorTabItem : EditorTabItem {

        private BitmapImage PiLogoBitmap = new(new Uri("../Assets/Icons/PiLogo.png", UriKind.Relative));

        public BoardEditorTabItem(string localFilePath) : base(localFilePath) {
            Image piLogo = new() {
                Source = PiLogoBitmap,
                ToolTip = "File on Board",
            };

            IconsStackPanel.Children.Add(piLogo);
        }
    }
}
