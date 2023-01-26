﻿using System.Windows;
using System.Windows.Controls;

namespace PiIDE {

    public partial class OptionsWindow : Window {
        public OptionsWindow() {
            InitializeComponent();

            int port = GlobalSettings.Default.SelectedCOMPort;
            if (port >= 0) {
                COMPortComboBox.Items.Add(new TextBlock() {
                    Text = $"COM{port}",
                    Tag = port
                });
                COMPortComboBox.SelectedIndex = 1;
            }
        }

        private void COMPortComboBox_DropDownOpened(object sender, System.EventArgs e) {
            COMPortComboBox.Items.Clear();
            COMPortComboBox.Items.Add("<None>");
            foreach (int port in Tools.GetCOMPorts())
                COMPortComboBox.Items.Add(new TextBlock() {
                    Text = $"COM{port}",
                    Tag = port
                });
            COMPortComboBox.SelectedIndex = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

            int selectedCOMPort;

            if (COMPortComboBox.SelectedIndex <= 0)
                selectedCOMPort = -1;
            else
                selectedCOMPort = (int) ((TextBlock) COMPortComboBox.SelectedItem).Tag;

            GlobalSettings.Default.SelectedCOMPort = selectedCOMPort;
            GlobalSettings.Default.Save();
        }
    }
}
