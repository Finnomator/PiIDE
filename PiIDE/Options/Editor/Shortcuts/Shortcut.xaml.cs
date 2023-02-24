using Humanizer;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace PiIDE.Options.Editor.Shortcuts {
    public partial class Shortcut : UserControl {

        private bool AllKeysReleased = true;
        public PiIDE.Shortcut IShortcut { get; private set; }
        private List<Key> Hotkey;

        public Shortcut(PiIDE.Shortcut shortcut) {
            InitializeComponent();
            IShortcut = shortcut;
            Hotkey = PiIDE.Shortcuts.ShortcutsMap[shortcut];
            ShortcutLabel.Content = shortcut.ToString().Humanize(LetterCasing.Title);
            ShortcutTextBox.Text = string.Join(" + ", Hotkey);
        }

        private void ShortcutTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            e.Handled = true;

            if (Hotkey.Count > 0 && Hotkey[^1] == e.Key)
                return;

            if (AllKeysReleased) {
                Hotkey.Clear();
                Hotkey.Add(e.Key);
            } else {
                Hotkey.Add(e.Key);
            }

            ShortcutTextBox.Text = string.Join(" + ", Hotkey);
            PiIDE.Shortcuts.ShortcutsMap[IShortcut] = Hotkey;
            AllKeysReleased = false;
        }

        private void ShortcutTextBox_KeyUp(object sender, KeyEventArgs e) {
            AllKeysReleased = !PiIDE.Shortcuts.IsAnyKeyPressed();
        }

        private void Reset_Click(object sender, System.Windows.RoutedEventArgs e) {
            Hotkey = PiIDE.Shortcuts.DefaultShortcutsMap[IShortcut];
            ShortcutTextBox.Text = string.Join(" + ", Hotkey);
            PiIDE.Shortcuts.ShortcutsMap[IShortcut] = PiIDE.Shortcuts.ShortcutsMap[IShortcut];
        }

        private void ShortcutTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            ResetButton.IsEnabled = !PiIDE.Shortcuts.DefaultShortcutsMap[IShortcut].SequenceEqual(Hotkey);
        }
    }
}
