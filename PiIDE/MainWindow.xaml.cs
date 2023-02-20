using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PiIDE {

    public partial class MainWindow : Window {

        private FontAwesome.WPF.FontAwesome MaximizeIcon = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.WindowMaximize };
        private FontAwesome.WPF.FontAwesome RestoreIcon = new() { Icon = FontAwesome.WPF.FontAwesomeIcon.WindowRestore };
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

        private void Minimize_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e) {
            if (!Editor.AreAllFilesSaved()) {
                MessageBoxResult msgbr = MessageBox.Show("Some files are not saved. Exit anyway?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (msgbr == MessageBoxResult.No)
                    return;
            }

            GlobalSettings.Default.Save();
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Point relativeMousePos = e.GetPosition(TitleBarRow);
            Point absoluteMousePos = PointToScreen(relativeMousePos);
            if (e.ChangedButton == MouseButton.Left && DraggableRect.Contains(relativeMousePos)) {
                if (WindowState != WindowState.Normal) {
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
            PaddingBorder.BorderThickness = new(0);
        }
    }
}
