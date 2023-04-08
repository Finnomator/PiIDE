using PiIDE.Assets.Icons;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PiIDE.Editor.Parts {
    public class BoardEditorTabItem : EditorTabItem {

        private readonly BitmapImage PiLogoBitmap = Icons.GetIcon("RaspberryPi");

        public BoardEditorTabItem(string localFilePath) : base(localFilePath) {
            Image piLogo = new() {
                Source = PiLogoBitmap,
                ToolTip = "File on Board",
            };

            IconsStackPanel.Children.Add(piLogo);
        }
    }
}
