using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace _1809_UWP
{
    public sealed partial class NavbarControl : UserControl
    {
        public NavbarControl()
        {
            this.InitializeComponent();
            ApplyBackdropOrAcrylic();
        }

        private void ApplyBackdropOrAcrylic()
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 12))
            {
            muxc: BackdropMaterial.SetApplyToRootOrPageBackground(this, true);
            }
            else
            {
                this.Background = new AcrylicBrush
                {
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                    TintColor = Colors.Transparent,
                    TintOpacity = 0.6,
                    FallbackColor = Colors.Gray
                };
            }
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
                mainPage.MainContentFrame.Navigate(typeof(SettingsPage));
            }
        }
    }
}
