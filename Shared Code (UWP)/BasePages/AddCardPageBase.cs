using System;
using System.Linq;
using Shared_Code;
using Shared_Code_UWP.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using ZXing;

namespace Shared_Code_UWP.BasePages
{
    public abstract class AddCardPageBase : Page
    {
        protected abstract TextBox CardNameEntry { get; }
        protected abstract TextBox CardNicknameEntry { get; }
        protected abstract TextBox CardNumberEntry { get; }
        protected abstract ComboBox DisplayPicker { get; }
        protected Frame PageFrame => this.Frame;

        public interface IScannerResult
        {
            string Text { get; }
            BarcodeFormat Format { get; }
        }

        protected void InitializeBarcodeFormatPicker()
        {
            var supportedTypes = Enum.GetValues(typeof(BarcodeFormat))
                                     .Cast<BarcodeFormat>()
                                     .Where(dt => BarcodeHelper.IsSupportedDisplayType(dt))
                                     .ToList();
            DisplayPicker.ItemsSource = supportedTypes;
            DisplayPicker.SelectedIndex = -1;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                var lastScanResult = GetLastScanResult();
                if (lastScanResult != null)
                {
                    CardNumberEntry.Text = lastScanResult.Text;
                    if (lastScanResult.Format != default(BarcodeFormat) && BarcodeHelper.IsSupportedDisplayType(lastScanResult.Format))
                    {
                        DisplayPicker.SelectedItem = lastScanResult.Format;
                    }
                    else if (!string.IsNullOrEmpty(lastScanResult.Text))
                    {
                        await DialogService.ShowErrorDialogAsync(this, "Unrecognized or unsupported barcode format.");
                    }
                    ClearLastScanResult();
                }
            }
        }

        protected abstract IScannerResult GetLastScanResult();
        protected abstract void ClearLastScanResult();
        protected abstract Type GetScannerPageType();

        protected async void AddCardButton_Click(object sender, RoutedEventArgs e)
        {
            string cardName = CardNameEntry.Text;
            string cardNickname = CardNicknameEntry.Text;
            string cardNumberText = CardNumberEntry.Text;

            if (!(DisplayPicker.SelectedItem is BarcodeFormat selectedDisplayType) || selectedDisplayType == default)
            {
                await DialogService.ShowErrorDialogAsync(this, "Please select a display type.");
                return;
            }

            if (string.IsNullOrWhiteSpace(cardName) || string.IsNullOrWhiteSpace(cardNumberText))
            {
                await DialogService.ShowErrorDialogAsync(this, "Please fill all required fields (Name and Number).");
                return;
            }

            if (!BarcodeHelper.ValidateBarcode(cardNumberText, selectedDisplayType, out string errorMessage))
            {
                await DialogService.ShowErrorDialogAsync(this, $"Invalid card number for the selected display type: {errorMessage}");
                return;
            }

            var newCard = new Card
            {
                CardName = cardName,
                CardNickname = cardNickname,
                CardNumber = cardNumberText,
                DisplayType = selectedDisplayType
            };

            try
            {
                CardRepository.Instance.AddCard(newCard);
                if (PageFrame.CanGoBack)
                {
                    PageFrame.GoBack();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding card: {ex.Message}");
                await DialogService.ShowErrorDialogAsync(this, $"Failed to add card: {ex.Message}");
            }
        }

        protected void ScanCardButton_Click(object sender, RoutedEventArgs e)
        {
            Type scannerPageType = GetScannerPageType();
            if (scannerPageType != null)
            {
                PageFrame.Navigate(scannerPageType);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ScannerPage type not specified in derived class.");
                _ = DialogService.ShowErrorDialogAsync(this, "Cannot navigate to scanner page.");
            }
        }
    }
}