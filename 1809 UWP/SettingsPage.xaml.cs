using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System;
using Shared_Code;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;
using Windows.UI;
using Microsoft.UI.Xaml.Media;

namespace _1809_UWP
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            ApplyBackdropOrAcrylic();
        }

        private void ApplyBackdropOrAcrylic()
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 12))
            {
            muxc: BackdropMaterial.SetApplyToRootOrPageBackground(this, true);
            }
            else
            {
                this.Background = new AcrylicBrush
                {
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                    TintColor = Colors.Transparent,
                    TintOpacity = 0.6,
                    FallbackColor = Colors.Gray
                };
            }
        }

        private async void ResetApp_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Confirm Reset",
                Content = "Are you sure you want to reset the app? This will delete all cards.",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                CardRepository.Instance.Cards.Clear();
                CardRepository.Instance.Database.DeleteAll<Card>();
            }
        }

        private async void ImportCards_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Import Cards",
                PrimaryButtonText = "Paste Text",
                SecondaryButtonText = "Scan Barcode",
                CloseButtonText = "Cancel"
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var textBox = new TextBox
                {
                    PlaceholderText = "Paste QR code content here",
                    AcceptsReturn = true,
                    Height = 100,
                    TextWrapping = TextWrapping.Wrap
                };

                var inputDialog = new ContentDialog
                {
                    Title = "Paste QR Code Content",
                    Content = textBox,
                    PrimaryButtonText = "Import",
                    CloseButtonText = "Cancel"
                };

                var inputResult = await inputDialog.ShowAsync();

                if (inputResult == ContentDialogResult.Primary)
                {
                    string importedText = textBox.Text;
                    await ImportCardsFromText(importedText);
                }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                var fileOpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
                fileOpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                fileOpenPicker.FileTypeFilter.Add(".jpg");
                fileOpenPicker.FileTypeFilter.Add(".jpeg");
                fileOpenPicker.FileTypeFilter.Add(".png");

                var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.VideoCapture);

                if (devices.Count > 0)
                {
                    var cameraCapture = new Windows.Media.Capture.CameraCaptureUI();
                    cameraCapture.PhotoSettings.Format = Windows.Media.Capture.CameraCaptureUIPhotoFormat.Jpeg;
                    cameraCapture.PhotoSettings.CroppedSizeInPixels = new Windows.Foundation.Size(200, 200);

                    var photo = await cameraCapture.CaptureFileAsync(Windows.Media.Capture.CameraCaptureUIMode.Photo);

                    if (photo != null)
                    {
                        var stream = await photo.OpenAsync(Windows.Storage.FileAccessMode.Read);
                        var bitmapDecoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                        var bitmap = await bitmapDecoder.GetSoftwareBitmapAsync();

                        var barcodeReader = new BarcodeReader();
                        var scanResult = barcodeReader.Decode(bitmap);

                        if (scanResult != null)
                        {
                            await ImportCardsFromText(scanResult.Text);
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
                else
                {
                    var file = await fileOpenPicker.PickSingleFileAsync();
                    if (file != null)
                    {
                        var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                        var bitmapDecoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
                        var bitmap = await bitmapDecoder.GetSoftwareBitmapAsync();

                        var barcodeReader = new BarcodeReader();
                        var scanResult = barcodeReader.Decode(bitmap);

                        if (scanResult != null)
                        {
                            await ImportCardsFromText(scanResult.Text);
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

        private async Task ImportCardsFromText(string importedText)
        {
            progressRing.IsActive = true;
            try
            {
                var importedCards = JsonConvert.DeserializeObject<ObservableCollection<Card>>(importedText);
                foreach (var card in importedCards)
                {
                    CardRepository.Instance.AddCard(card);
                }
                progressRing.IsActive = false;
                await new ContentDialog
                {
                    Title = "Success",
                    Content = "Cards imported successfully!",
                    CloseButtonText = "OK"
                }.ShowAsync();
            }
            catch (Exception ex)
            {
                progressRing.IsActive = false;
                await new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to import cards: {ex.Message}",
                    CloseButtonText = "OK"
                }.ShowAsync();
            }
        }

        private async void ExportCards_Click(object sender, RoutedEventArgs e)
        {
            progressRing.IsActive = true;

            var cards = CardRepository.Instance.Cards;
            var serializedCards = JsonConvert.SerializeObject(cards);
            var qrCodeBitmap = await GenerateQrCodeAsync(serializedCards);

            progressRing.IsActive = false;

            var image = new Image
            {
                Source = qrCodeBitmap,
                Stretch = Windows.UI.Xaml.Media.Stretch.Uniform,
                Width = 200,
                Height = 200
            };

            var dialog = new ContentDialog
            {
                Title = "Exported QR Code",
                Content = new ScrollViewer
                {
                    Content = image
                },
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }

        private async Task<WriteableBitmap> GenerateQrCodeAsync(string content)
        {
            return await Task.Run(async () =>
            {
                BarcodeWriterPixelData writer = new BarcodeWriterPixelData
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new EncodingOptions
                    {
                        Height = 200,
                        Width = 200,
                        Margin = 1
                    }
                };

                var pixelData = writer.Write(content);

                WriteableBitmap bitmap = null;
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    bitmap = new WriteableBitmap(pixelData.Width, pixelData.Height);
                    using (var stream = bitmap.PixelBuffer.AsStream())
                    {
                        stream.Write(pixelData.Pixels, 0, pixelData.Pixels.Length);
                    }
                });

                return bitmap;
            });
        }
    }
}
