using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Shared_Code;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using System;

namespace _1709_UWP
{
    public sealed partial class CardDetailPage : Page
    {
        public CardDetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Card selectedCard)
            {
                this.DataContext = selectedCard;

                var barcodeBitmap = GenerateBarcode(selectedCard.CardNumber.ToString(), selectedCard.DisplayType);
                barcodeImage.Source = barcodeBitmap;
            }
        }

        private WriteableBitmap GenerateBarcode(string value, DisplayType displayType)
        {
            BarcodeWriterPixelData writer = new BarcodeWriterPixelData
            {
                Format = displayType == DisplayType.Bar128 ? BarcodeFormat.CODE_128 : BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 200,
                    Width = 200,
                    Margin = 1
                }
            };

            var pixelData = writer.Write(value);
            WriteableBitmap bitmap = new WriteableBitmap(pixelData.Width, pixelData.Height);

            using (var stream = bitmap.PixelBuffer.AsStream())
            {
                stream.Write(pixelData.Pixels, 0, pixelData.Pixels.Length);
            }

            return bitmap;
        }

        private void EditCard_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Card selectedCard)
            {
                Frame.Navigate(typeof(EditCardPage), selectedCard);
            }
        }

        private async void DeleteCard_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Delete Card",
                Content = "Are you sure you want to delete this card?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary && DataContext is Card selectedCard)
            {
                CardRepository.Instance.DeleteCard(selectedCard);
                Frame.GoBack();
            }
        }
    }
}
