﻿using PiIDE.Options.Editor.SyntaxHighlighter.Colors;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace PiIDE;

public static class ColorResources {

    public static readonly Brush AccentColorBrush = (Brush) Application.Current.Resources["AccentBrush"];

    public static Dictionary<string, Brush> LoadResource(string filePath) {
        using Stream stream = File.OpenRead(filePath);
        return LoadResource(stream);
    }

    public static Dictionary<string, Brush> LoadResource(Stream stream) {

        Dictionary<string, Brush> result = new();

        string content;

        using (StreamReader reader = new(stream)) {
            content = reader.ReadToEnd();
        }

        Dictionary<string, string> deserialized = JsonSerializer.Deserialize<Dictionary<string, string>>(content)!;

        foreach (string key in deserialized.Keys) {
            Brush converted = (Brush) Tools.BrushConverter.ConvertFromString(deserialized[key])!;
            result[key] = converted;
        }

        return result;
    }

    public static void SaveResource(string filePath, Dictionary<string, Brush> resource) {
        using Stream stream = File.Open(filePath, FileMode.Open);
        stream.SetLength(0);
        SaveResource(stream, resource);
    }

    public static void SaveResource(Stream stream, Dictionary<string, Brush> resource) {

        Dictionary<string, string> serializable = new();
        foreach (string key in resource.Keys)
            serializable[key] = resource[key].ToString();

        string serialized = JsonSerializer.Serialize(serializable);
        using StreamWriter writer = new(stream);
        writer.Write(serialized);
    }

    public static class HighlighterColors {

        public static Dictionary<string, Brush> ColorsMap { get; private set; } = LoadResource(ColorOptions.ColorsJsonPath);
        public static Dictionary<string, Brush> DefaultColors { get; private set; } = LoadResource(ColorOptions.DefaultColorsJsonPath);

        public static Brush Class => ColorsMap["class"];
        public static Brush Function => ColorsMap["function"];
        public static Brush Instance => ColorsMap["instance"]; // What is this?
        public static Brush Statement => ColorsMap["statement"];
        public static Brush Module => ColorsMap["module"];
        public static Brush Param => ColorsMap["param"];
        public static Brush Property => ColorsMap["property"]; // What is this?
        public static Brush Path => ColorsMap["path"]; // What is this?
        public static Brush Namespace => ColorsMap["namespace"]; // What is this?

        public static Brush Comment => ColorsMap["comment"];
        public static Brush String => ColorsMap["string"];
        public static Brush Number => ColorsMap["number"];
        public static Brush Keyword => ColorsMap["keyword"];

        public static readonly Brush EverythingElse = Brushes.Gray;

        public static Brush GetBrush(string key) {
            if (ColorsMap.TryGetValue(key, out Brush? value))
                return value;
#if DEBUG
            MessageBox.Show($"Type '{key}' not found");
#endif
            return EverythingElse;
        }

        public static void SetBrush(string key, Brush value) => ColorsMap[key] = value;

        public static void SetColors(Dictionary<string, Brush> colors) => ColorsMap = colors;
    }
}