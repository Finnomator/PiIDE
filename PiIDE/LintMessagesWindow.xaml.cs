using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PiIDE {

    public partial class LintMessagesWindow : UserControl {

        private double OldScrollState;
        private ScrollViewer ListViewScrollViewer;

        public LintMessagesWindow() {
            InitializeComponent();
        }

        public void ClearLintMessages() => MainListView.ItemsSource = null;

        public void AddLintMessages(PylintMessage[] messages) {
            MainListView.ItemsSource = messages;

            CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(MainListView.ItemsSource);
            PropertyGroupDescription groupDescription = new("Module");
            view.GroupDescriptions.Add(groupDescription);

            ListViewScrollViewer.ScrollToVerticalOffset(OldScrollState);
        }

        public async void UpdateLintMessages(string[] filesToLint) {
            PylintMessage[] pylintMessages = await PylintWraper.GetLintingAsync(filesToLint);
            ClearLintMessages();
            AddLintMessages(pylintMessages);
        }

        private ScrollViewer? FindScrollViewer(DependencyObject d) {
            if (d is ScrollViewer)
                return d as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++) {
                var sw = FindScrollViewer(VisualTreeHelper.GetChild(d, i));
                if (sw != null)
                    return sw;
            }
            return null;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            ListViewScrollViewer = FindScrollViewer(MainListView);
            ListViewScrollViewer.ScrollChanged += (s, e) => OldScrollState = e.VerticalOffset;
        }
    }
}
