using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace PiIDE {
    internal static class Shortcuts {

        // TODO: add all keys to this array
        private static readonly Key[] _keys = Enum.GetValues(typeof(Key)).Cast<Key>().ToArray();
        public static Dictionary<Shortcut, List<Key>> ShortcutsMap;
        public static Dictionary<Shortcut, List<Key>> DefaultShortcutsMap = new() {
            { Shortcut.SaveFile, new() { Key.LeftCtrl, Key.S } },
            { Shortcut.OpenCompletionsList, new() { Key.LeftCtrl, Key.Space } },
            { Shortcut.OpenSearchBox, new() { Key.LeftCtrl, Key.F} },
            { Shortcut.FormatDocument, new() { Key.LeftShift, Key.LeftCtrl, Key.F} },
        };
        public static readonly Shortcut[] DefaultShortcuts = Enum.GetValues(typeof(Shortcut)).Cast<Shortcut>().ToArray();

        static Shortcuts() {
            ShortcutsMap = LoadShortcuts(Options.Editor.Shortcuts.Shortcuts.ShortcutsJsonPath);
        }

        public static bool IsAnyKeyPressed() {
            for (int i = 1; i < _keys.Length; i++) {
                Key key = _keys[i];
                if (Keyboard.IsKeyDown(key))
                    return true;
            }

            return false;
        }
        public static bool AreKeysPressed(List<Key> keys) => keys.All(Keyboard.IsKeyDown);
        public static bool AreTheOnlyKeysPressed(List<Key> keys) {
            for (int i = 1; i < _keys.Length; ++i) {
                Key key = _keys[i];
                if (Keyboard.IsKeyDown(key) && !keys.Contains(key))
                    return false;
            }
            return AreKeysPressed(keys);
        }
        public static bool IsShortcutPressed(Shortcut shortcut) {
            if (ShortcutsMap.ContainsKey(shortcut))
                return AreTheOnlyKeysPressed(ShortcutsMap[shortcut]);
            return false;
        }

        public static bool IsTheOnlyKeyPressed(Key key) {
            for (int i = 1; i < _keys.Length; i++) {
                Key k = _keys[i];
                if (Keyboard.IsKeyDown(k) && k != key)
                    return false;
            }
            return true;
        }

        public static Dictionary<Shortcut, List<Key>> LoadShortcuts(string filePath) {
            using Stream stream = File.OpenRead(filePath);
            return LoadShortcuts(stream);
        }

        public static Dictionary<Shortcut, List<Key>> LoadShortcuts(Stream stream) {

            string content;

            using (StreamReader reader = new(stream))
                content = reader.ReadToEnd();

            Dictionary<Shortcut, List<string>> deserialized = JsonSerializer.Deserialize<Dictionary<Shortcut, List<string>>>(content)!;

            Dictionary<Shortcut, List<Key>> result = new();
            foreach (Shortcut key in deserialized.Keys)
                result[key] = deserialized[key].Select(Enum.Parse<Key>).ToList();

            return result;
        }

        public static void SaveShortcuts(string filePath, Dictionary<Shortcut, List<Key>> shortcuts) {
            using Stream stream = File.Open(filePath, FileMode.Open);
            stream.SetLength(0);
            SaveShortcuts(stream, shortcuts);
        }

        public static void SaveShortcuts(Stream stream, Dictionary<Shortcut, List<Key>> shortcuts) {
            Dictionary<Shortcut, List<string>> serializable = new();
            foreach (Shortcut key in shortcuts.Keys)
                serializable[key] = shortcuts[key].Select(x => x.ToString()).ToList();
            string serialized = JsonSerializer.Serialize(serializable);
            using StreamWriter writer = new(stream);
            writer.Write(serialized);
        }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Shortcut {
        SaveFile,
        OpenCompletionsList,
        OpenSearchBox,
        FormatDocument,
    }
}
