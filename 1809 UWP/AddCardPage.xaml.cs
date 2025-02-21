using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Shared_Code;
using System;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace _1809_UWP
{
    public sealed partial class AddCardPage : Page
    {
        public AddCardPage()
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

        private async void OnAddCard(object sender, RoutedEventArgs e)
        {
            string cardName = cardNameEntry.Text;
            string cardNickname = cardNicknameEntry.Text;
            string cardNumberText = cardNumberEntry.Text;
            ComboBoxItem selectedDisplayTypeItem = displayPicker.SelectedItem as ComboBoxItem;

            if (string.IsNullOrWhiteSpace(cardName) || string.IsNullOrWhiteSpace(cardNumberText) || selectedDisplayTypeItem == null)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Please fill all required fields.",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();
                return;
            }

            if (!int.TryParse(cardNumberText, out int cardNumber))
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Card number must be a valid integer.",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();
                return;
            }

            if (!Enum.TryParse(selectedDisplayTypeItem.Content.ToString(), out DisplayType selectedDisplayType))
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Invalid display type selected.",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();
                return;
            }

            var newCard = new Card
            {
                CardName = cardName,
                CardNickname = cardNickname,
                CardNumber = cardNumber,
                DisplayType = selectedDisplayType
            };

            CardRepository.Instance.AddCard(newCard);

            Frame.Navigate(typeof(MainPage));
        }
    }
}
