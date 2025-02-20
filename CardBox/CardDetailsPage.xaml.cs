using Shared_Code;
using ZXing.Net.Maui;

namespace CardBox
{
    public partial class CardDetailPage : ContentPage
    {
        public CardDetailPage(Card selectedCard)
        {
            InitializeComponent();
            BindingContext = selectedCard;

            barcodeImage.Format = selectedCard.DisplayType switch
            {
                DisplayType.Bar128 => BarcodeFormat.Code128,
                DisplayType.QrCode => BarcodeFormat.QrCode,
                _ => BarcodeFormat.QrCode
            };
        }
    }
}
