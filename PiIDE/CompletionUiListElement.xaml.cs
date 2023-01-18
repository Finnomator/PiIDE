using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE {

    public partial class CompletionUiListElement : UserControl {

        public readonly Completion Completion;
        public bool IsSelected { get; private set; }

        private readonly Brush SelectedBrush = Tools.SelectedBrush;
        private readonly Brush UnselectedBrush = Tools.UnselectedBrush;

        public CompletionUiListElement(Completion completion) {
            InitializeComponent();

            Completion = completion;

            MainButton.Text = Completion.Name;
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
