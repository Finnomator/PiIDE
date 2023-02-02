using System.Windows;
using System.Windows.Controls;

namespace PiIDE {
    public class FileTabItem : TabItem {

        public FileTabItem() {
            Style = (Style) Application.Current.FindResource("FileTabItemStyle");

        }
    }
}
