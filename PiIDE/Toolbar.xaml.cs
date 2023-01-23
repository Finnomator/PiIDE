using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PiIDE {

    public partial class Toolbar : UserControl {

        public EventHandler? OnOpenOptionsCLick;

        public Toolbar() {
            InitializeComponent();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e) => OnOpenOptionsCLick?.Invoke(sender, e);
    }
}
