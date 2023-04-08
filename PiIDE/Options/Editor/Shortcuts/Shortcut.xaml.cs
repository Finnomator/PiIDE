using Humanizer;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace PiIDE.Options.Editor.Shortcuts;

public partial class Shortcut {

    private bool AllKeysReleased = true;
    public PiIDE.Shortcut TheShortcut { get; }
    private List<Key> Hotkey;

    public Shortcut(PiIDE.Shortcut theShortcut) {
        InitializeComponent();
        TheShortcut = theShortcut;
        Hotkey = PiIDE.Shortcuts.ShortcutsMap[theShortcut];
        ShortcutLabel.Content = theShortcut.ToString().Humanize(LetterCasing.Title);
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
        PiIDE.Shortcuts.ShortcutsMap[TheShortcut] = Hotkey;
        AllKeysReleased = false;
    }

    private void ShortcutTextBox_KeyUp(object sender, KeyEventArgs e) => AllKeysReleased = !PiIDE.Shortcuts.IsAnyKeyPressed();

    private void Reset_Click(object sender, System.Windows.RoutedEventArgs e) {
        Hotkey = PiIDE.Shortcuts.DefaultShortcutsMap[TheShortcut];
        ShortcutTextBox.Text = string.Join(" + ", Hotkey);
        PiIDE.Shortcuts.ShortcutsMap[TheShortcut] = PiIDE.Shortcuts.ShortcutsMap[TheShortcut];
    }

    private void ShortcutTextBox_TextChanged(object sender, TextChangedEventArgs e) => ResetButton.IsEnabled = !PiIDE.Shortcuts.DefaultShortcutsMap[TheShortcut].SequenceEqual(Hotkey);
}