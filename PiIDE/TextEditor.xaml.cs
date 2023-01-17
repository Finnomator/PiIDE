using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;

namespace PiIDE {

    public partial class TextEditor : UserControl {

        public TextEditor() {
            InitializeComponent();

            TextListBox.ItemsSource = GetDataSet();
        }

        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e) {

        }

        public ArrayList GetDataSet() {
            ArrayList items = new ArrayList();
            for (var count = 0; count < 1000000; ++count) {
                items.Add(string.Format("Item {0}", count));
            }
            return items;
        }

        public Point CaretPosition() => new();
    }
}
