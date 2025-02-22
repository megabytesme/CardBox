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

namespace _1709_UWP
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
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
            var textBox = new TextBox
            {
                PlaceholderText = "Paste QR code content here",
                AcceptsReturn = true,
                Height = 100,
                TextWrapping = TextWrapping.Wrap
            };

            var dialog = new ContentDialog
            {
                Title = "Import Cards",
                Content = textBox,
                PrimaryButtonText = "Import",
                CloseButtonText = "Cancel"
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                progressRing.IsActive = true;
                string importedText = textBox.Text;
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
