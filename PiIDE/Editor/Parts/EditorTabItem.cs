using FontAwesome.WPF;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Editor.Parts {
    public class EditorTabItem : TabItem {

        public string FilePath { get; private set; }
        public string FileName { get; private set; }

        public Button SaveLocalButton { get; private set; }
        public Button CloseTabButton { get; private set; }

        public event EventHandler<string>? SaveLocalClick;
        public event EventHandler<string>? CloseTabClick;

        public EditorTabItem(string filePath) {

            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);

            TextBlock fileNameTextBlock = new() {
                Text = FileName,
            };

            WrapPanel header = new();

            CloseTabButton = new() {
                Content = new FontAwesome.WPF.FontAwesome() {
                    Icon = FontAwesomeIcon.Remove
                },
                Background = Brushes.Transparent,
                BorderBrush = null,
                Padding = new(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            CloseTabButton.Click += (s, e) => CloseTabClick?.Invoke(this, FilePath);

            SaveLocalButton = new() {
                Content = new FontAwesome.WPF.FontAwesome() {
                    Icon = FontAwesomeIcon.Save
                },
                Background = Brushes.Transparent,
                BorderBrush = null,
                Padding = new(0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "Save Localy"
            };
            SaveLocalButton.Click += (s, e) => SaveLocalClick?.Invoke(this, FilePath);

            header.Children.Add(fileNameTextBlock);
            header.Children.Add(new Border() { BorderThickness = new(3) });
            header.Children.Add(SaveLocalButton);
            header.Children.Add(CloseTabButton);

            Header = header;
        }
    }
}
