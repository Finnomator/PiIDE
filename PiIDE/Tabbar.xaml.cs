using System;
using System.Collections.Generic;
using System.IO;
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

namespace PiIDE {

    public partial class Tabbar : UserControl {

        private TabItem OpenTabItem;

        public Tabbar() {
            InitializeComponent();
            OpenTabItem = DefaultEditor;
        }

        public void AddFile(string filePath) {
            TextEditor textEditor = new(filePath);
            MainTabControl.Items.Add(new TabItem() {
                Header = Path.GetFileName(filePath),
                Content = textEditor,
            });
        }

        public void AddTempFile() {
            string newFilePath = $"TempFiles/temp_file{Directory.GetFiles("TempFiles").Length}.py";
            File.Create(newFilePath);
            AddFile(newFilePath);
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            TextEditor oldTextEditor = (TextEditor) OpenTabItem.Content;
            oldTextEditor.DisablAllWrapers = true;

            OpenTabItem = (TabItem) MainTabControl.SelectedItem;
            TextEditor newTextEditor = (TextEditor) OpenTabItem.Content;
            newTextEditor.DisablAllWrapers = false;
        }
    }
}
