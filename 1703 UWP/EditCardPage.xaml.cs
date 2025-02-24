using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Shared_Code;
using Windows.UI.Xaml;
using ZXing;

namespace _1703_UWP
{
    public sealed partial class EditCardPage : Page
    {
        public EditCardPage()
        {
            this.InitializeComponent();
            var supportedTypes = Enum.GetValues(typeof(BarcodeFormat))
                                                 .Cast<BarcodeFormat>()
                                                 .Where(dt => BarcodeHelper.IsSupportedDisplayType(dt))
                                                 .ToList();
            displayPicker.ItemsSource = supportedTypes;
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
                if (displayPicker.SelectedItem is BarcodeFormat selectedDisplayType)
                {
                    selectedCard.DisplayType = selectedDisplayType;
                }

                if (BarcodeHelper.ValidateBarcode(selectedCard.CardNumber, selectedCard.DisplayType, out string errorMessage))
                {
                    CardRepository.Instance.EditCard(selectedCard);
                    Frame.GoBack();
                }
                else
                {
                    DisplayErrorMessage(errorMessage);
                }
            }
        }

        private void DisplayErrorMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Validation Error",
                Content = message,
                CloseButtonText = "OK"
            };
            _ = dialog.ShowAsync();
        }
    }
}