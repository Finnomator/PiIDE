using System.Windows.Controls;

namespace PiIDE.Editor.Parts.Explorer {
    /// <summary>
    /// This is the abstract base for explorers
    /// </summary>
    public abstract partial class ExplorerBase : UserControl {
        public ExplorerBase() {
            InitializeComponent();
        }
    }
}
