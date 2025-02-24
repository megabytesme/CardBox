using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using ZXing;
using ZXing.Common;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System;
using Shared_Code;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;
using Windows.UI;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Documents;

namespace _1809_UWP
{
    public sealed partial class SettingsPage : Page
    {
        private readonly CardImportExport _importExport = new CardImportExport();

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.Back)
            {
                ScannerPage.ScannerResult result = ScannerPage.LastScanResult;

                if (result != null)
                {
                    ImportCardsFromBarcodeResult(result);
                    ScannerPage.LastScanResult = null;
                }
            }
        }

        private async void ImportCardsFromBarcodeResult(ScannerPage.ScannerResult result)
        {
            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                await ImportCardsFromTextAsync(result.Text);
            }
            else
            {
                await ShowErrorDialog("Barcode scan failed or was cancelled.");
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
                    await ImportCardsFromTextAsync(importedText);
                }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                Frame.Navigate(typeof(ScannerPage));
            }
        }

        private async Task ImportCardsFromTextAsync(string importedText)
        {
            progressRing.IsActive = true;
            try
            {
                var importedCards = _importExport.ImportCardsFromTextAsync(importedText, out List<Card> invalidCards);

                if (importedCards != null)
                {
                    foreach (var card in importedCards)
                    {
                        CardRepository.Instance.Cards.Add(card);
                        CardRepository.Instance.Database.Insert(card);
                    }

                    progressRing.IsActive = false;

                    if (invalidCards.Count > 0)
                    {
                        string invalidCardsMessage = "The following cards are invalid and have not been added:\n" +
                                                     string.Join("\n", invalidCards.Select(c => c.CardName));
                        await ShowWarningDialog(invalidCardsMessage);
                    }
                    else
                    {
                        await ShowSuccessDialog("Cards imported successfully!");
                    }
                }
                else
                {
                    progressRing.IsActive = false;
                    await ShowErrorDialog("Failed to import cards. Invalid data.");
                }
            }
            catch (Exception ex)
            {
                progressRing.IsActive = false;
                await ShowErrorDialog($"Failed to import cards: {ex.Message}");
            }
        }

        private async Task ShowWarningDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Warning",
                Content = message,
                CloseButtonText = "OK"
            };
            await dialog.ShowAsync();
        }

        private async void ExportCards_Click(object sender, RoutedEventArgs e)
        {
            progressRing.IsActive = true;

            var cards = CardRepository.Instance.Cards;
            string exportedText = _importExport.ExportCardsToText(cards);

            var qrCodeBitmap = await GenerateQrCodeAsync(exportedText);

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
                Content = new ScrollViewer { Content = image },
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }

        private async Task ShowErrorDialog(string message)
        {
            await new ContentDialog { Title = "Error", Content = message, CloseButtonText = "OK" }.ShowAsync();
        }

        private async Task ShowSuccessDialog(string message)
        {
            await new ContentDialog { Title = "Success", Content = message, CloseButtonText = "OK" }.ShowAsync();
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
                        Height = 400,
                        Width = 400,
                        Margin = 0
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

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "About CardBox Tool",
                Content = new ScrollViewer()
                {
                    Content = new TextBlock()
                    {
                        Inlines =
                        {
                            new Run() { Text = "CardBox Tool" },
                            new LineBreak(),
                            new Run() { Text = "Version 1.0.0.1 (1809_UWP)" },
                            new LineBreak(),
                            new Run() { Text = "Copyright © 2025 MegaBytesMe" },
                            new LineBreak(),
                            new Run() { Text = " "},
                            new LineBreak(),
                            new Run() { Text = "Source code available on " },
                            new Hyperlink()
                            {
                                NavigateUri = new Uri("https://github.com/megabytesme/CardBox"),
                                Inlines = { new Run() { Text = "GitHub" } }
                            },
                            new LineBreak(),
                            new Run() { Text = "Anything wrong? Let us know: " },
                            new Hyperlink()
                            {
                                NavigateUri = new Uri("https://github.com/megabytesme/CardBox/issues"),
                                Inlines = { new Run() { Text = "Support" } }
                            },
                            new LineBreak(),
                            new Run() { Text = "Privacy Policy: " },
                            new Hyperlink()
                            {
                                NavigateUri = new Uri("https://github.com/megabytesme/CardBox/blob/master/PRIVACYPOLICY.md"),
                                Inlines = { new Run() { Text = "Privacy Policy" } }
                            },
                            new LineBreak(),
                            new Run() { Text = "Like what you see? View my " },
                            new Hyperlink()
                            {
                                NavigateUri = new Uri("https://github.com/megabytesme"),
                                Inlines = { new Run() { Text = "GitHub" } }
                            },
                            new Run() { Text = " and maybe my " },
                            new Hyperlink()
                            {
                                NavigateUri = new Uri("https://apps.microsoft.com/search?query=megabytesme"),
                                Inlines = { new Run() { Text = "Other Apps" } }
                            },
                            new LineBreak(),
                            new Run() { Text = " "},
                            new LineBreak(),
                            new Run() { Text = "CardBox is designed to help you manage your loyalty cards effortlessly." }
                        },
                        TextWrapping = TextWrapping.Wrap
                    }
                },
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }
    }
}
