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

    public partial class CompletionUiList : UserControl {

        public int SelectedCompletionIndex { get; private set; }

        public CompletionUiList() {
            InitializeComponent();
        }

        public void AddCompletions(List<Completion> completions) {
            for (int i = 0; i < completions.Count; i++) {
                Completion completion = completions[i];
                CompletionUiListElement completionUiListElement = new(completion);

                CompletionsStackPanel.Children.Add(completionUiListElement);
            }
        }

        public void ClearCompletions() => CompletionsStackPanel.Children.Clear();

        public void MoveSelectedCompletionUp() {
            if (SelectedCompletionIndex == 0)
                SelectedCompletionIndex = CompletionsStackPanel.Children.Count;
            ((CompletionUiListElement) CompletionsStackPanel.Children[SelectedCompletionIndex]).Deselect();
            --SelectedCompletionIndex;
            ((CompletionUiListElement) CompletionsStackPanel.Children[SelectedCompletionIndex]).Select();
        }

        public void MoveSelectedCompletionDown() {
            if (SelectedCompletionIndex == CompletionsStackPanel.Children.Count - 1)
                SelectedCompletionIndex = -1;
            ((CompletionUiListElement) CompletionsStackPanel.Children[SelectedCompletionIndex]).Deselect();
            ++SelectedCompletionIndex;
            ((CompletionUiListElement) CompletionsStackPanel.Children[SelectedCompletionIndex]).Select();
        }
    }
}
