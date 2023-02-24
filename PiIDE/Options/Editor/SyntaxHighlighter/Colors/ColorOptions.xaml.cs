﻿using Microsoft.Win32;
using PiIDE.Options.Editor.SyntaxHighlighter.Colors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Options.Editor.SyntaxHighlighter.Colors {

    public partial class ColorOptions : UserControl {

        public const string ColorsJsonPath = "Options/Editor/SyntaxHighlighter/Colors/Colors.json";
        public const string DefaultColorsJsonPath = "Options/Editor/SyntaxHighlighter/Colors/DefaultColors.json";

        public ColorOptions() {
            InitializeComponent();

            ImportTheme(File.OpenRead(ColorsJsonPath));
        }

        private void ImportTheme(Stream stream) {
            ColorResources.HighlighterColors.SetColors(ColorResources.LoadResource(stream));

            MainStackPanel.Children.Clear();
            foreach (string key in ColorResources.HighlighterColors.Colors.Keys)
                MainStackPanel.Children.Add(new ColorOption(key));
        }

        private void ExportTheme_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog fileDialog = new() {
                Filter = "json files (*.json)|*.json",
            };

            fileDialog.ShowDialog();
            if (fileDialog.FileName == "")
                return;

            ColorResources.SaveResource(fileDialog.OpenFile(), ColorResources.HighlighterColors.Colors);
        }

        private void ImportTheme_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog fileDialog = new() {
                Filter = "json files (*.json)|*.json",
            };

            fileDialog.ShowDialog();
            if (fileDialog.FileName == "")
                return;

            ImportTheme(fileDialog.OpenFile());
        }
    }
}
