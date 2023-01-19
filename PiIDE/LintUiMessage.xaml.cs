using System.Windows.Controls;

namespace PiIDE {

    public partial class LintUiMessage : UserControl {

        public readonly PylintMessage PylintMessage;

        public LintUiMessage(PylintMessage pylintMessage) {
            InitializeComponent();
            PylintMessage = pylintMessage;

            TypeLabel.Content = PylintMessage.Type;
            MessageLabel.Content = PylintMessage.Message;
        }
    }
}
