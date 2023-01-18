using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE {

    public partial class CompletionUiListElement : UserControl {

        public readonly Completion Completion;
        public bool IsSelected { get; private set; }

        private readonly Brush SelectedBrush = Tools.SelectedBrush;
        private readonly Brush HighlightBrush = Tools.HighlightBrush;
        private readonly Brush UnselectedBrush = Tools.UnselectedBrush;

        public CompletionUiListElement(Completion completion) {
            InitializeComponent();

            Completion = completion;

            MainButton.Text = Completion.Name;

            if (TypeColors.TypeToColorMap.ContainsKey(completion.Type))
                MainButton.Foreground = TypeColors.TypeToColorMap[completion.Type];

            Debug.WriteLine($"{completion.Name}: {completion.Type}");
        }

        public void Select() {
            IsSelected = true;
            MainButton.Background = SelectedBrush;
        }

        public void Deselect() {
            IsSelected = false;
            MainButton.Background = UnselectedBrush;
        }

        public void Highlight() => MainButton.Background = HighlightBrush;
    }
}
