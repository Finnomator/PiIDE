using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PiIDE {

    public partial class CompletionUiListElement : UserControl {

        public readonly Completion Completion;
        public bool IsSelected { get; private set; }

        private readonly Brush SelectedBrush;
        private readonly Brush UnselectedBrush;

        public CompletionUiListElement(Completion completion) {
            InitializeComponent();

            Completion = completion;

            SelectedBrush = Brushes.DarkGray;
            UnselectedBrush = Brushes.Gray;

            MainButton.Content = Completion.Name;
        }

        public void Select() {
            IsSelected = true;
            MainButton.Foreground = SelectedBrush;
        }

        public void Deselect() {
            IsSelected = false;
            MainButton.Foreground = UnselectedBrush;
        }
    }
}
