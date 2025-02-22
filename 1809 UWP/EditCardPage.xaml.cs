using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Shared_Code;
using Windows.UI.Xaml;
using System;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;
using Windows.UI;
using Microsoft.UI.Xaml.Media;

namespace _1809_UWP
{
    public sealed partial class EditCardPage : Page
    {
        public EditCardPage()
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Card selectedCard)
            {
                this.DataContext = selectedCard;
            }
        }

        private void SaveCard_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Card selectedCard)
            {
                if (displayPicker.SelectedItem is ComboBoxItem selectedDisplayTypeItem &&
                    Enum.TryParse(selectedDisplayTypeItem.Content.ToString(), out DisplayType selectedDisplayType))
                {
                    selectedCard.DisplayType = selectedDisplayType;
                }
                CardRepository.Instance.EditCard(selectedCard);
                Frame.GoBack();
            }
        }
    }
}
