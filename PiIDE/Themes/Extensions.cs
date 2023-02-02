using System.Windows;
using System.Windows.Controls;

namespace PiIDE.Themes {

    public static class Extensions {

        public static readonly DependencyProperty IsSavedLocalyProperty = DependencyProperty.RegisterAttached("IsSavedLocaly", typeof(bool), typeof(Extensions), new PropertyMetadata(false));

        public static void SetIsSavedLocaly(TabItem element, bool value) => element.SetValue(IsSavedLocalyProperty, value);

        public static bool GetIsSavedLocaly(TabItem element) => (bool) element.GetValue(IsSavedLocalyProperty);
    }
}
