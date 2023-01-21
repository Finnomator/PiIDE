using System.Threading.Tasks;
using System.Windows.Controls;

namespace PiIDE {

    public partial class CompletionUiList : UserControl {

        public int SelectedCompletionIndex { get; private set; }
        public bool SelectedAnIndex { get; private set; }
        public bool IsOpen { get; private set; }

        private readonly string FilePath;

        public CompletionUiList(string filePath) {
            InitializeComponent();
            FilePath = filePath;
        }

        public Completion SelectedCompletion => GetCompletion(SelectedCompletionIndex);
        public CompletionUiListElement SelectedUiCompletion => GetUiCompletion(SelectedCompletionIndex);
        public int CompletionsCount => CompletionsStackPanel.Children.Count;

        public Completion GetCompletion(int index) => GetUiCompletion(index).Completion;
        public CompletionUiListElement GetUiCompletion(int index) => (CompletionUiListElement) CompletionsStackPanel.Children[index];

        private void AddCompletions(Completion[] completions) {
            SelectedCompletionIndex = 0;
            for (int i = 0; i < completions.Length; ++i) {
                Completion completion = completions[i];
                CompletionUiListElement completionUiListElement = new(completion);
                CompletionsStackPanel.Children.Add(completionUiListElement);
            }

            SelectFirstCompletion();
        }

        private void SelectFirstCompletion() {
            SelectedCompletionIndex = 0;
            SelectedAnIndex = true;
            SelectedUiCompletion.Select();
        }

        private void ClearCompletions() {
            CompletionsStackPanel.Children.Clear();
            SelectedAnIndex = false;
            SelectedCompletionIndex = 0;
        }

        public async Task ReloadCompletionsAsync(int caretLine, int caretColumn) {
            ClearCompletions();
            Completion[] completions = await JediCompletionWraper.GetCompletionAsync(FilePath, caretLine, caretColumn);

            if (completions.Length == 0) {
                Close();
                return;
            }

            AddCompletions(completions);
        }

        public void Close() => ClearCompletions();

        public void MoveSelectedCompletionUp() {

            if (SelectedCompletionIndex == 0) {
                if (SelectedAnIndex)
                    SelectedUiCompletion.Deselect();
                SelectedCompletionIndex = CompletionsCount - 1;
                MainScrollViewer.ScrollToTop();
                SelectedUiCompletion.Select();
            } else if (SelectedAnIndex) {
                SelectedUiCompletion.Deselect();
                --SelectedCompletionIndex;
                SelectedUiCompletion.Select();
            }

            SelectedAnIndex = true;
        }

        public void MoveSelectedCompletionDown() {

            if (SelectedCompletionIndex == CompletionsCount - 1) {
                if (SelectedAnIndex)
                    SelectedUiCompletion.Deselect();
                SelectedCompletionIndex = 0;
                MainScrollViewer.ScrollToTop();
                SelectedUiCompletion.Select();
            } else if (SelectedAnIndex) {
                SelectedUiCompletion.Deselect();
                ++SelectedCompletionIndex;
                SelectedUiCompletion.Select();
            }

            SelectedAnIndex = true;
        }
    }
}
