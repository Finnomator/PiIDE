using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace PiIDE {
    internal static class Shortcuts {

        // TODO: add all keys to this array
        private static readonly Key[] _keys = Enum.GetValues(typeof(Key)).Cast<Key>().ToArray();

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
        public static bool IsTheOnlyKeyPressed(Key key) {
            for (int i = 1; i < _keys.Length; i++) {
                Key k = _keys[i];
                if (Keyboard.IsKeyDown(k) && k != key)
                    return false;
            }
            return true;
        }
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
