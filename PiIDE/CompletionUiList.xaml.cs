using System.Threading.Tasks;
using System.Windows.Controls;

namespace PiIDE {

    public partial class CompletionUiList : UserControl {

        public int SelectedCompletionIndex { get; private set; }
        public bool SelectedAnIndex { get; private set; }
        public bool IsOpen { get; private set; }

        private const double CompletionUiListElementHeight = 15.96;
        private readonly string FilePath;

        public CompletionUiList(string filePath) {
            InitializeComponent();
            FilePath = filePath;
            Close();
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

        public async Task ReloadCompletionsAsync(string fileContent, int caretLine, int caretColumn) {
            ClearCompletions();
            Completion[] completions = await JediCompletionWraper.GetCompletionAsync(FilePath, fileContent, caretLine, caretColumn);

            if (completions.Length == 0) {
                Close();
                return;
            }

            AddCompletions(completions);
        }

        public void Close() => ClearCompletions();

        public void MoveSelectedCompletionUp() {

            if (CompletionsStackPanel.Children.Count == 0)
                return;

            if (SelectedCompletionIndex == 0) {
                if (SelectedAnIndex)
                    SelectedUiCompletion.Deselect();
                SelectedCompletionIndex = CompletionsCount - 1;
                SelectedUiCompletion.Select();
            } else if (SelectedAnIndex) {
                SelectedUiCompletion.Deselect();
                --SelectedCompletionIndex;
                SelectedUiCompletion.Select();
            }

            MainScrollViewer.ScrollToVerticalOffset(SelectedCompletionIndex * CompletionUiListElementHeight);

            SelectedAnIndex = true;
        }

        public void MoveSelectedCompletionDown() {

            if (CompletionsStackPanel.Children.Count == 0)
                return;

            if (SelectedCompletionIndex == CompletionsCount - 1) {
                if (SelectedAnIndex)
                    SelectedUiCompletion.Deselect();
                SelectedCompletionIndex = 0;
                SelectedUiCompletion.Select();
            } else if (SelectedAnIndex) {
                SelectedUiCompletion.Deselect();
                ++SelectedCompletionIndex;
                SelectedUiCompletion.Select();
            }

            MainScrollViewer.ScrollToVerticalOffset(SelectedCompletionIndex * CompletionUiListElementHeight);

            SelectedAnIndex = true;
        }
    }
}
