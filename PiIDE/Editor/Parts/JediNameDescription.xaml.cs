using PiIDE.Wrapers;
using System.Windows.Controls;
using System.Windows;
using JediName = PiIDE.Wrapers.JediWraper.ReturnClasses.Name;
using System.Threading.Tasks;

namespace PiIDE.Editor.Parts {

    public partial class JediNameDescription : UserControl {

        public bool CloseWhenMouseLeaves { get; set; }

        public JediNameDescription() {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

        public async void OpenAsync(JediName jediName) {
            Visibility = Visibility.Visible;
            TypeTextBlock.Text = jediName.Type;
            TypeTextBlock.Foreground = jediName.Foreground;
            NameTextBlock.Text = jediName.Name;

            DocstringWraper.Visibility = Visibility.Visible;
            DocstringSeperator.Visibility = Visibility.Visible;
            DocstringTextBox.Visibility = Visibility.Visible;
            
            LoadingDocstringState();
            LoadingTypeHintState();

            string? typeHint = await jediName.GetTypeHint();

            TypeHintLabel.Content = string.IsNullOrEmpty(typeHint) ? "Any" : typeHint;

            string? docstring = await jediName.Docstring();

            if (string.IsNullOrEmpty(docstring)) {
                DocstringSeperator.Visibility = Visibility.Collapsed;
                DocstringWraper.Visibility = Visibility.Collapsed;
            } else {
                DocstringTextBox.Text = docstring;
                DocstringSpinner.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadingTypeHintState() => TypeHintLabel.Content = Tools.FontAwesome_Loading;
        private void LoadingDocstringState() {
            DocstringWraper.Visibility = Visibility.Visible;
            DocstringSpinner.Visibility = Visibility.Visible;
            DocstringTextBox.Text = "";
        }

        public void Close() => Visibility = Visibility.Collapsed;

        public void CloseIfNotMouseOver() {
            if (!IsMouseOver)
                Close();
        }

        private void Control_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            if (CloseWhenMouseLeaves)
                Close();
        }
    }
}
