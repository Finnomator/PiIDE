using System.Windows.Controls;

namespace PiIDE {

    public partial class CompletionUiList : UserControl {

        public int SelectedCompletionIndex { get; private set; }
        public bool SelectedAnIndex { get; private set; }

        public CompletionUiList() {
            InitializeComponent();
        }

        public Completion SelectedCompletion => GetCompletion(SelectedCompletionIndex);
        public CompletionUiListElement SelectedUiCompletion => GetUiCompletion(SelectedCompletionIndex);
        public CompletionUiListElement HighlightedUiCompletion => GetUiCompletion(0);
        public Completion HighlightedCompletion => HighlightedUiCompletion.Completion;
        public int CompletionsCount => CompletionsStackPanel.Children.Count;

        public Completion GetCompletion(int index) => GetUiCompletion(index).Completion;
        public CompletionUiListElement GetUiCompletion(int index) => (CompletionUiListElement) CompletionsStackPanel.Children[index];

        public void AddCompletions(Completion[] completions) {
            SelectedCompletionIndex = 0;
            for (int i = 0; i < completions.Length; ++i) {
                Completion completion = completions[i];
                CompletionUiListElement completionUiListElement = new(completion);
                CompletionsStackPanel.Children.Add(completionUiListElement);
            }

            if (completions.Length < 10) {
                HighlightedUiCompletion.Highlight();
                SelectedCompletionIndex = 1;
            }
        }

        public void ClearCompletions() {
            CompletionsStackPanel.Children.Clear();
            SelectedAnIndex = false;
            SelectedCompletionIndex = 0;
        }

        public void MoveSelectedCompletionUp() {

            HighlightedUiCompletion.Deselect();

            if (SelectedCompletionIndex == 0) {
                if (SelectedAnIndex)
                    SelectedUiCompletion.Deselect();
                SelectedCompletionIndex = CompletionsCount - 1;
                MainScrollViewer.ScrollToTop();
            } else if (SelectedAnIndex) {
                SelectedUiCompletion.Deselect();
                --SelectedCompletionIndex;
            }
            SelectedUiCompletion.Select();

            SelectedAnIndex = true;
        }

        public void MoveSelectedCompletionDown() {

            HighlightedUiCompletion.Deselect();

            if (SelectedCompletionIndex == CompletionsCount - 1) {
                if (SelectedAnIndex)
                    SelectedUiCompletion.Deselect();
                SelectedCompletionIndex = 0;
                MainScrollViewer.ScrollToTop();
            } else if (SelectedAnIndex) {
                SelectedUiCompletion.Deselect();
                ++SelectedCompletionIndex;
            }
            SelectedUiCompletion.Select();

            SelectedAnIndex = true;
        }
    }
}
