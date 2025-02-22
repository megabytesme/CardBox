using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Shared_Code;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;
using Windows.UI;
using Microsoft.UI.Xaml.Media;

namespace _1809_UWP
{
    public sealed partial class HomePage : Page
    {
        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;

        public HomePage()
        {
            this.InitializeComponent();
            DataContext = this;
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

        private void ViewCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Card selectedCard)
            {
                Frame.Navigate(typeof(CardDetailPage), selectedCard);
            }
        }
    }
}
