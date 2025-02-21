using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace _1809_UWP
{
    public sealed partial class NavbarControl : UserControl
    {
        public NavbarControl()
        {
            this.InitializeComponent();
        }

        private void OnHomeClicked(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }

        private void OnAddClicked(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(AddCardPage));
        }

        private void OnSettingsClicked(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            // rootFrame.Navigate(typeof(SettingsPage));
        }
    }
}
