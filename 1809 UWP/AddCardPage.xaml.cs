using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Shared_Code;
using System;
using ZXing;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace _1809_UWP
{
    public sealed partial class AddCardPage : Page
    {
        public AddCardPage()
        {
            this.InitializeComponent();
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
                CardNumber = cardNumberText,
                DisplayType = selectedDisplayType
            };

            CardRepository.Instance.AddCard(newCard);

            Frame.GoBack();
        }

        private async void OnScanCard(object sender, RoutedEventArgs e)
        {
            var cameraCapture = new CameraCaptureUI();
            cameraCapture.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            cameraCapture.PhotoSettings.CroppedSizeInPixels = new Windows.Foundation.Size(200, 200);

            var photo = await cameraCapture.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (photo != null)
            {
                var stream = await photo.OpenAsync(FileAccessMode.Read);
                var bitmapDecoder = await BitmapDecoder.CreateAsync(stream);
                var bitmap = await bitmapDecoder.GetSoftwareBitmapAsync();

                var barcodeReader = new BarcodeReader();
                var result = barcodeReader.Decode(bitmap);

                if (result != null)
                {
                    cardNumberEntry.Text = result.Text;
                    if (result.BarcodeFormat == BarcodeFormat.QR_CODE)
                    {
                        displayPicker.SelectedItem = displayPicker.Items[0];
                    }
                    else if (result.BarcodeFormat == BarcodeFormat.CODE_128)
                    {
                        displayPicker.SelectedItem = displayPicker.Items[1];
                    }
                }
                else
                {
                    await new ContentDialog
                    {
                        Title = "Error",
                        Content = "Failed to read barcode.",
                        CloseButtonText = "OK"
                    }.ShowAsync();
                }
            }
        }
    }
}
