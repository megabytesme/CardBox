using Shared_Code;
using ZXing;

namespace CardBox
{
    public partial class EditCardPage : ContentPage
    {
        private Card _selectedCard;

        public EditCardPage(Card selectedCard)
        {
            InitializeComponent();
            _selectedCard = selectedCard;
            var supportedTypes = Enum.GetValues(typeof(BarcodeFormat))
                                           .Cast<BarcodeFormat>()
                                           .Where(dt => BarcodeHelper.IsSupportedDisplayType(dt))
                                           .ToList();
            picker.ItemsSource = supportedTypes;
            BindingContext = _selectedCard;
            picker.SelectedItem = _selectedCard.DisplayType;
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void SaveCard_Click(object sender, EventArgs e)
        {
            if (BindingContext is Card selectedCard)
            {
                if (picker.SelectedItem is BarcodeFormat selectedDisplayType)
                {
                    selectedCard.DisplayType = selectedDisplayType;
                }

                if (BarcodeHelper.ValidateBarcode(selectedCard.CardNumber, selectedCard.DisplayType, out string errorMessage))
                {
                    CardRepository.Instance.EditCard(selectedCard);
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayErrorMessage(errorMessage);
                }
            }
        }

        private async Task DisplayErrorMessage(string message)
        {
            await DisplayAlert("Validation Error", message, "OK");
        }
    }
}