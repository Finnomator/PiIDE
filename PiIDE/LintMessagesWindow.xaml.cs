using System.Windows.Controls;

namespace PiIDE {

    public partial class LintMessagesWindow : UserControl {

        public LintMessagesWindow() {
            InitializeComponent();
        }

        public void ClearLintMessages() => LintMessagesStackPanel.Children.Clear();

        public void AddLintMessages(PylintMessage[] messages) {
            for (int i = 0; i < messages.Length; ++i) {
                LintUiMessage lintUiMessage = new(messages[i]);
                LintMessagesStackPanel.Children.Add(lintUiMessage);
            }
        }
    }
}
