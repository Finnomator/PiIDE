using PiIDE.Wrapers;
using System.Windows;
using System.Windows.Input;

namespace PiIDE {

    public partial class MainWindow : Window {

        private readonly FontAwesome.WPF.FontAwesome MaximizeIcon = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.WindowMaximize };
        private readonly FontAwesome.WPF.FontAwesome RestoreIcon = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.WindowRestore };
        private Rect DraggableRect => new(0, 0, ActualWidth, TitleBarRow.ActualHeight);

        public MainWindow() {
            InitializeComponent();
            MissingModulesChecker.CheckForUsableModules();
        }

        private void Maximize_Click(object sender, RoutedEventArgs e) {

            if (WindowState == WindowState.Normal) {
                MaximizeWindow();
            } else if (WindowState == WindowState.Maximized) {
                NormalizeWindow();
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private async void Close_Click(object sender, RoutedEventArgs e) {
            Editor.StopAllEditorAutoSaving();
            await Editor.SaveAllFilesAsync();
            GlobalSettings.Default.Save();
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Point relativeMousePos = e.GetPosition(TitleBarRow);

            if (e.ChangedButton == MouseButton.Left && DraggableRect.Contains(relativeMousePos)) {
                if (WindowState != WindowState.Normal) {
                    Point absoluteMousePos = PointToScreen(relativeMousePos).ConvertToDevice();
                    NormalizeWindow();
                    Left = absoluteMousePos.X - ActualWidth / 2;
                    Top = absoluteMousePos.Y - TitleBarRow.ActualHeight / 2;
                }

                DragMove();
            }
        }

        private void MaximizeWindow() {
            WindowState = WindowState.Maximized;
            MaximizeButton.Content = RestoreIcon;
            PaddingBorder.BorderThickness = new(8);
        }

        private void NormalizeWindow() {
            WindowState = WindowState.Normal;
            MaximizeButton.Content = MaximizeIcon;
            PaddingBorder.BorderThickness = new(2);
        }

        private void Window_StateChanged(object sender, System.EventArgs e) {
            // Because the window can maximize without clicking the maximize button
            if (WindowState == WindowState.Maximized)
                MaximizeWindow();
        }
    }
}
