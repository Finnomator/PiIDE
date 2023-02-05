using PiIDE.Wrapers;
using System.Windows.Controls;
using System.Windows;

namespace PiIDE.Editor.Parts {

    public partial class JediNameDescription : UserControl {

        public bool CloseWhenMouseLeaves { get; set; }

        public JediNameDescription() {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

        public void Open(JediName jediName) {
            Visibility = Visibility.Visible;
            TypeTextBlock.Text = jediName.Type;
            TypeTextBlock.Foreground = jediName.ForegroundColor;
            NameTextBlock.Text = jediName.Name;

            if (jediName.TypeHint is not null) {
                TypeHintTextBlock.Text = jediName.TypeHint;
                TypeHintTextBlock.Visibility = Visibility.Visible;
                TypeHintSeperatorTextBlock.Visibility = Visibility.Visible;
            } else {
                TypeHintTextBlock.Visibility = Visibility.Collapsed;
                TypeHintSeperatorTextBlock.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(jediName.Docstring)) {
                DocstringSeperator.Visibility = Visibility.Collapsed;
                DocstringTextBlock.Visibility = Visibility.Collapsed;
            } else {
                DocstringTextBlock.Text = jediName.Docstring;
                DocstringSeperator.Visibility = Visibility.Visible;
                DocstringTextBlock.Visibility = Visibility.Visible;
            }
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
