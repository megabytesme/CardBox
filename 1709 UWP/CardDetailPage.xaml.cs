using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Shared_Code;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using System.Runtime.InteropServices.WindowsRuntime;

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
    }
}
