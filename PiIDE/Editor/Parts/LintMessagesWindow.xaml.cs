using PiIDE.Wrappers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace PiIDE.Editor.Parts;

public partial class LintMessagesWindow {

    private double OldScrollState;
    private ScrollViewer? ListViewScrollViewer;
    private const string WikidotBaseUrl = "http://pylint-messages.wikidot.com/messages:";

    public EventHandler<PylintMessage>? SelectionChanged;

    public LintMessagesWindow() => InitializeComponent();

    private PylintMessage? SelectedMessage => (PylintMessage?) MainListView.SelectedItem;

    private void ClearLintMessages() => MainListView.ItemsSource = null;

    private void AddLintMessages(PylintMessage[] messages) {
        MainListView.ItemsSource = messages;

        CollectionView view = (CollectionView) CollectionViewSource.GetDefaultView(MainListView.ItemsSource);
        PropertyGroupDescription groupDescription = new("Path");
        view.GroupDescriptions!.Add(groupDescription);

        ListViewScrollViewer?.ScrollToVerticalOffset(OldScrollState);
    }

    public async Task<PylintMessage[]> UpdateLintMessages(string[] filesToLint) {

        SetIntoLoadingState();
        PylintMessage[] pylintMessages = await PylintWrapper.GetLintingAsync(filesToLint);
        ResetToNormalState();

        ClearLintMessages();
        AddLintMessages(pylintMessages);

        return pylintMessages;
    }

    private void ResetToNormalState() => PylintStatus.Visibility = Visibility.Collapsed;

    private void SetIntoLoadingState() => PylintStatus.Visibility = Visibility.Visible;

    private void UserControl_Loaded(object sender, RoutedEventArgs _) {
        ListViewScrollViewer = Tools.FindScrollViewer(MainListView);
        ListViewScrollViewer.ScrollChanged += (_, e) => OldScrollState = e.VerticalOffset;
    }

    private void MainListViewSelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (SelectedMessage != null) {
            SelectionChanged?.Invoke(this, SelectedMessage);
        }
        // TODO: Maybe add a click effect
        MainListView.SelectedIndex = -1;
    }

    private void CodeHyperlink_Click(object sender, RoutedEventArgs e) {
        Hyperlink hyperlink = (Hyperlink) sender;
        string code = (string) hyperlink.Tag;
        string errorUrl = WikidotBaseUrl + code;
        try {
            Process.Start(new ProcessStartInfo(errorUrl) { UseShellExecute = true });
            // ReSharper disable once RedundantCatchClause
        } catch {
#if DEBUG
            throw;
#endif
        }
    }
}