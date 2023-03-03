using FontAwesome.WPF;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PiIDE.Editor.Parts {
    public class EditorTabItem : TabItem {

        public string FilePath { get; private set; }
        public string FileName { get; private set; }

        public Button SaveLocalButton { get; private set; }
        public Button CloseTabButton { get; private set; }

        public event EventHandler<string>? SaveLocalClick;
        public event EventHandler<string>? CloseTabClick;

        protected new WrapPanel Header;
        protected StackPanel IconsStackPanel;

        private readonly BitmapImage PythonLogoBitmap = new(new Uri("../Assets/Icons/Python.png", UriKind.Relative));
        private readonly BitmapImage TextFileBitmap = new(new Uri("../Assets/Icons/FileIcon.png", UriKind.Relative));

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
                Source = Tools.IsPythonFile(filePath) ? PythonLogoBitmap : TextFileBitmap,
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

            Header.Children.Add(IconsStackPanel);
            Header.Children.Add(fileNameTextBlock);
            Header.Children.Add(new Border() { BorderThickness = new(3) });
            Header.Children.Add(SaveLocalButton);
            Header.Children.Add(CloseTabButton);

            base.Header = Header;
        }
    }
}
