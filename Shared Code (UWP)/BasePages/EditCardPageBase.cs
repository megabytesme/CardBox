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
    public abstract class EditCardPageBase : Page
    {
        protected Card CurrentCard { get; private set; }

        protected abstract ComboBox DisplayPicker { get; }

        protected Frame PageFrame => this.Frame;

        protected void InitializeBarcodeFormatPicker()
        {
            var supportedTypes = Enum.GetValues(typeof(BarcodeFormat))
                                     .Cast<BarcodeFormat>()
                                     .Where(dt => BarcodeHelper.IsSupportedDisplayType(dt))
                                     .ToList();
            DisplayPicker.ItemsSource = supportedTypes;
            if (CurrentCard != null)
            {
                DisplayPicker.SelectedItem = CurrentCard.DisplayType;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Card selectedCard)
            {
                CurrentCard = selectedCard;
                this.DataContext = CurrentCard;
                InitializeBarcodeFormatPicker();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("EditCardPageBase: Invalid navigation parameter.");
                if (PageFrame.CanGoBack) PageFrame.GoBack();
            }
        }

        protected async void SaveCardButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCard == null) return;

            if (DisplayPicker.SelectedItem is BarcodeFormat selectedDisplayType)
            {
                if (CurrentCard.DisplayType != selectedDisplayType)
                {
                    CurrentCard.DisplayType = selectedDisplayType;
                }
            }
            else
            {
                await DialogService.ShowErrorDialogAsync(this, "Please select a valid display type.");
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentCard.CardName) || string.IsNullOrWhiteSpace(CurrentCard.CardNumber))
            {
                await DialogService.ShowErrorDialogAsync(this, "Card Name and Card Number cannot be empty.");
                return;
            }

            if (BarcodeHelper.ValidateBarcode(CurrentCard.CardNumber, CurrentCard.DisplayType, out string errorMessage))
            {
                try
                {
                    CardRepository.Instance.EditCard(CurrentCard);
                    if (PageFrame.CanGoBack)
                    {
                        PageFrame.GoBack();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving card: {ex.Message}");
                    await DialogService.ShowErrorDialogAsync(this, $"Failed to save changes: {ex.Message}");
                }
            }
            else
            {
                await DialogService.ShowErrorDialogAsync(this, $"Invalid card number for the selected display type: {errorMessage}", "Validation Error");
            }
        }
    }
}