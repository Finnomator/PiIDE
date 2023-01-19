using System.Windows;
using System;
using System.Windows.Controls;

namespace PiIDE {

    public partial class LintMessagesWindow : UserControl {

        public LintMessagesWindow() {
            InitializeComponent();
        }

        public void ClearLintMessages() => MainListView.ItemsSource = null;

        public void AddLintMessages(PylintMessage[] messages) => MainListView.ItemsSource = messages;
    }
}
