using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Shared_Code;
using System;
using ZXing;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Graphics.Imaging;
using System.Linq;
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

        private async void OnScanCard(object sender, RoutedEventArgs e)
        {
            var cameraCapture = new CameraCaptureUI
            {
                PhotoSettings =
                {
                    Format = CameraCaptureUIPhotoFormat.Jpeg,
                    CroppedSizeInPixels = new Windows.Foundation.Size(200, 200)
                }
            };

            var photo = await cameraCapture.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (photo != null)
            {
                using (var stream = await photo.OpenAsync(FileAccessMode.Read))
                {
                    var bitmapDecoder = await BitmapDecoder.CreateAsync(stream);
                    var bitmap = await bitmapDecoder.GetSoftwareBitmapAsync();
                    var barcodeReader = new BarcodeReader();
                    var result = barcodeReader.Decode(bitmap);

                    if (result != null)
                    {
                        cardNumberEntry.Text = result.Text;
                        displayPicker.SelectedItem = result.BarcodeFormat;
                    }
                    else
                    {
                        await ShowErrorDialog("Failed to read barcode.");
                    }
                }
            }
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
