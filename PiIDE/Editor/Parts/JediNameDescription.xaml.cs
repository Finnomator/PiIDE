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

        public async Task OpenAsync(JediName jediName) {
            Visibility = Visibility.Visible;
            TypeTextBlock.Text = jediName.Type;
            TypeTextBlock.Foreground = jediName.Foreground;
            NameTextBlock.Text = jediName.Name;

            string? typeHint = await jediName.GetTypeHint();
            string? docstring = await jediName.Docstring();

            if (jediName.GetTypeHint() is not null) {
                TypeHintTextBlock.Text = typeHint;
                TypeHintTextBlock.Visibility = Visibility.Visible;
                TypeHintSeperatorTextBlock.Visibility = Visibility.Visible;
            } else {
                TypeHintTextBlock.Visibility = Visibility.Collapsed;
                TypeHintSeperatorTextBlock.Visibility = Visibility.Collapsed;
            }


            if (string.IsNullOrEmpty(docstring)) {
                DocstringSeperator.Visibility = Visibility.Collapsed;
                DocstringTextBlock.Visibility = Visibility.Collapsed;
            } else {
                DocstringTextBlock.Text = docstring;
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
