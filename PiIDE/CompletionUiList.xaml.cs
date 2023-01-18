using System.Windows.Controls;

namespace PiIDE {

    public partial class CompletionUiList : UserControl {

        public int SelectedCompletionIndex { get; private set; }
        public bool SelectedAnIndex { get; private set; }

        public CompletionUiList() {
            InitializeComponent();
        }

        public Completion SelectedCompletion => ((CompletionUiListElement)CompletionsStackPanel.Children[SelectedCompletionIndex]).Completion;

        public void AddCompletions(Completion[] completions) {
            for (int i = 0; i < completions.Length; ++i) {
                Completion completion = completions[i];
                CompletionUiListElement completionUiListElement = new(completion);
                CompletionsStackPanel.Children.Add(completionUiListElement);
            }
        }

        public void ClearCompletions() {
            CompletionsStackPanel.Children.Clear();
            SelectedAnIndex = false;
            SelectedCompletionIndex = 0;
        }

        public void MoveSelectedCompletionUp() {
            SelectedAnIndex = true;
            if (SelectedCompletionIndex == 0)
                SelectedCompletionIndex = CompletionsStackPanel.Children.Count;
            ((CompletionUiListElement) CompletionsStackPanel.Children[SelectedCompletionIndex]).Deselect();
            --SelectedCompletionIndex;
            ((CompletionUiListElement) CompletionsStackPanel.Children[SelectedCompletionIndex]).Select();
        }

        public void MoveSelectedCompletionDown() {
            SelectedAnIndex = true;
            //if (SelectedCompletionIndex == CompletionsStackPanel.Children.Count - 1)
             //   SelectedCompletionIndex = -1;
            ((CompletionUiListElement) CompletionsStackPanel.Children[SelectedCompletionIndex]).Deselect();
            ++SelectedCompletionIndex;
            ((CompletionUiListElement) CompletionsStackPanel.Children[SelectedCompletionIndex]).Select();
        }
    }
}
