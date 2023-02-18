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
            Height = 30;

            Style = (Style) Application.Current.Resources["TabItemStyle"];

            TextBlock fileNameTextBlock = new() {
                Text = FileName,
                Foreground = Brushes.White,
            };

            WrapPanel header = new();

            CloseTabButton = new() {
                Content = "⛌",
                FontSize = 9,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                ToolTip = "Close",
                Style = (Style) Application.Current.Resources["CleanButtonWithRoundCornersStyle"],
                Foreground = Brushes.White,
                Padding = new(2),
            };

            CloseTabButton.Click += (s, e) => CloseTabClick?.Invoke(this, FilePath);

            SaveLocalButton = new() {
                Content = new FontAwesome.WPF.FontAwesome() {
                    Icon = FontAwesomeIcon.Save
                },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "Save",
                Style = (Style) Application.Current.Resources["CleanButtonWithRoundCornersStyle"],
                Foreground = Brushes.White,
                Padding = new(2),
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
