using Humanizer;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Options.Editor.Shortcuts {
    public partial class Shortcuts : UserControl {

        public const string ShortcutsJsonPath = @"Options\Editor\Shortcuts\Shortcuts.json";

        public Shortcuts() {
            InitializeComponent();
            foreach (PiIDE.Shortcut shortcut in PiIDE.Shortcuts.ShortcutsMap.Keys) {
                AddShortcut(shortcut);
            }
        }

        private void AddShortcut(PiIDE.Shortcut cut) {

            if (!PiIDE.Shortcuts.ShortcutsMap.ContainsKey(cut))
                PiIDE.Shortcuts.ShortcutsMap[cut] = PiIDE.Shortcuts.DefaultShortcutsMap[cut];

            Grid grid = new();
            grid.ColumnDefinitions.Add(new());
            grid.ColumnDefinitions.Add(new() { Width = GridLength.Auto });

            Shortcut shortcut = new(cut);
            grid.Children.Add(shortcut);

            Button button = new() {
                Style = (Style) Application.Current.Resources["CleanButtonStyle"],
                Foreground = Brushes.White,
                Content = "Remove",
                Padding = new(7, 0, 7, 0),
            };
            button.Click += delegate {
                PiIDE.Shortcuts.ShortcutsMap.Remove(shortcut.IShortcut);
                MainStackPanel.Children.Remove(grid);
            };
            Grid.SetColumn(button, 1);
            grid.Children.Add(button);

            MainStackPanel.Children.Add(grid);
        }

        private List<PiIDE.Shortcut> GetUnusedShortcuts() {
            List<PiIDE.Shortcut> shortcuts = new();
            foreach (PiIDE.Shortcut shortcut in PiIDE.Shortcuts.DefaultShortcuts) {
                if (!MainStackPanel.Children.Cast<Grid>().Any(x => ((Shortcut) x.Children[0]).IShortcut == shortcut))
                    shortcuts.Add(shortcut);
            }
            return shortcuts;
        }

        private void Add_Click(object sender, RoutedEventArgs e) {
            if (SelectShortcutComboBox.SelectedIndex >= 0) {
                ShortcutComboBoxItem? selectedItem = ((ShortcutComboBoxItem?) SelectShortcutComboBox.SelectedItem);
                if (selectedItem == null)
                    return;
                AddShortcut(selectedItem.Shortcut);
            }
        }

        private void SelectShortcutComboBox_DropDownOpened(object sender, System.EventArgs e) => SelectShortcutComboBox.ItemsSource = GetUnusedShortcuts().Select(x => new ShortcutComboBoxItem(x)).ToArray();

        private class ShortcutComboBoxItem {

            public PiIDE.Shortcut Shortcut { get; init; }
            public string Content { get; init; }

            public ShortcutComboBoxItem(PiIDE.Shortcut shortcut) {
                Shortcut = shortcut;
                Content = shortcut.ToString().Humanize(LetterCasing.Title);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) => PiIDE.Shortcuts.SaveShortcuts(ShortcutsJsonPath, PiIDE.Shortcuts.ShortcutsMap);
    }
}
