using PiIDE.Assets.Icons;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PiIDE.Editor.Parts {
    public class EditorTabItem : TabItem {

        public string FilePath { get; }
        public string FileName { get; }

        public event EventHandler<string>? CloseTabClick;

        protected new WrapPanel Header;
        protected StackPanel IconsStackPanel;

        private readonly Button CloseTabButton;

        public EditorTabItem(string filePath) {

            FilePath = filePath;
            FileName = Path.GetFileName(FilePath);
            Height = 30;
            Header = new();

            Style = (Style) Application.Current.Resources["TabItemStyle"];

            IconsStackPanel = new() {
                MaxHeight = 16,
                Orientation = Orientation.Horizontal,
            };

            IconsStackPanel.Children.Add(new Image() {
                Source = Icons.GetFileIcon(FilePath),
            });

            TextBlock fileNameTextBlock = new() {
                Text = FileName,
                Foreground = Brushes.White,
            };

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

            Header.Children.Add(IconsStackPanel);
            Header.Children.Add(fileNameTextBlock);
            Header.Children.Add(new Border() { BorderThickness = new(3) });
            Header.Children.Add(CloseTabButton);

            base.Header = Header;
        }
    }
}
