using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PiIDE {

    public partial class Toolbar : UserControl {

        public EventHandler? OnCreateNewFileClick;
        public EventHandler<string>? OpenedFile;

        public Toolbar() {
            InitializeComponent();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void CreateNewFileClick(object sender, RoutedEventArgs e) => OnCreateNewFileClick?.Invoke(this, e);

        private void OpenFileClick(object sender, RoutedEventArgs e) {

            OpenFileDialog openFileDialog = new();

            if (openFileDialog.ShowDialog() == false) {
                return;
            }

            OpenedFile?.Invoke(sender, openFileDialog.FileName);
        }
    }
}
