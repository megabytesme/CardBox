using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Shared_Code;
using System;
using ZXing;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

namespace _1709_UWP
{
    public sealed partial class AddCardPage : Page
    {
        public AddCardPage()
        {
            this.InitializeComponent();
            var supportedTypes = Enum.GetValues(typeof(BarcodeFormat))
                                     .Cast<BarcodeFormat>()
                                     .Where(dt => BarcodeHelper.IsSupportedDisplayType(dt))
                                     .ToList();
            displayPicker.ItemsSource = supportedTypes;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.Back)
            {
                ScannerPage.ScannerResult result = ScannerPage.LastScanResult;

                if (result != null)
                {
                    cardNumberEntry.Text = result.Text;
                    if (result.Format != default(BarcodeFormat))
                    {
                        displayPicker.SelectedItem = result.Format;
                    }
                    else
                    {
                        await ShowErrorDialog("Unrecognized barcode format.");
                    }
                    ScannerPage.LastScanResult = null;
                }
            }
        }

        private async void OnAddCard(object sender, RoutedEventArgs e)
        {
            string cardName = cardNameEntry.Text;
            string cardNickname = cardNicknameEntry.Text;
            string cardNumberText = cardNumberEntry.Text;
            var selectedDisplayType = (BarcodeFormat)displayPicker.SelectedItem;

            if (string.IsNullOrWhiteSpace(cardName) || string.IsNullOrWhiteSpace(cardNumberText) || selectedDisplayType == default)
            {
                await ShowErrorDialog("Please fill all required fields.");
                return;
            }

            if (!BarcodeHelper.ValidateBarcode(cardNumberText, selectedDisplayType, out string errorMessage))
            {
                await ShowErrorDialog($"Invalid display type: {errorMessage}");
                return;
            }

            var newCard = new Card
            {
                CardName = cardName,
                CardNickname = cardNickname,
                CardNumber = cardNumberText,
                DisplayType = selectedDisplayType
            };

            CardRepository.Instance.AddCard(newCard);

            Frame.GoBack();
        }

        private void OnScanCard(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ScannerPage));
        }

        private async Task ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK"
            };
            await dialog.ShowAsync();
        }
    }
}