using System.Collections.Generic;
using System.Windows.Input;

namespace PiIDE {
    internal static class Shortcuts {
        public static bool AreKeysPressed(Key[] keys) {
            for (int i = 0; i < keys.Length; i++) {
                if (!Keyboard.IsKeyDown(keys[i]))
                    return false;
            }
            return true;
        }
        public static bool IsShortcutPressed(Key key1, Key key2) => Keyboard.IsKeyDown(key1) && Keyboard.IsKeyDown(key2);
        public static bool IsShortcutPressed((Key, Key) keys) => Keyboard.IsKeyDown(keys.Item1) && Keyboard.IsKeyDown(keys.Item2);
        public static bool IsShortcutPressed(Shortcut shortcut) => IsShortcutPressed(ShortcutMap.ShortcutsMap[shortcut]);
    }

    public enum Shortcut {
        SaveFile,
        OpenCompletionsList,
    }

    public readonly struct ShortcutMap {
        public static readonly Dictionary<Shortcut, (Key, Key)> ShortcutsMap = new() {
            {Shortcut.SaveFile,  (Key.LeftCtrl, Key.S)},
            {Shortcut.OpenCompletionsList, (Key.LeftCtrl, Key.Space) }
        };

        public ShortcutMap() {
        }
    }
}
