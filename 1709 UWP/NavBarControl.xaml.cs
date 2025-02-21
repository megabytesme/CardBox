using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace _1709_UWP
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
            if (rootFrame != null && rootFrame.Content is MainPage mainPage)
            {
                mainPage.MainContentFrame.Navigate(typeof(HomePage));
            }
        }

        private void OnAddClicked(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.Content is MainPage mainPage)
            {
                mainPage.MainContentFrame.Navigate(typeof(AddCardPage));
            }
        }

        private void OnSettingsClicked(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.Content is MainPage mainPage)
            {
                //mainPage.MainContentFrame.Navigate(typeof(SettingsPage));
            }
        }
    }
}
