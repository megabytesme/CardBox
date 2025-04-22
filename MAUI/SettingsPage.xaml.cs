using Shared_Code;
using ZXing.Net.Maui;
using ZXing.Common;
using BarcodeFormat = ZXing.Net.Maui.BarcodeFormat;
using ZXing.Net.Maui.Controls;
using ZXing;
using SkiaSharp;
using ZXing.SkiaSharp;
using Microsoft.Maui.Controls;

namespace CardBox
{
    public partial class SettingsPage : ContentPage
    {
        private readonly CardImportExport _importExport = new CardImportExport();
        private string _exportedResult;

        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public string ExportedResult
        {
            get => _exportedResult;
            set
            {
                if (_exportedResult != value)
                {
                    _exportedResult = value;
                    OnPropertyChanged();
                }
            }
        }

        private async void ResetApp_Click(object sender, EventArgs e)
        {
            bool result = await DisplayAlert("Confirm Reset", "Are you sure you want to reset the app? This will delete all cards.", "Yes", "No");
            if (result)
            {
                CardRepository.Instance.Cards.Clear();
                CardRepository.Instance.Database.DeleteAll<Card>();
            }
        }

        private async void ImportCards_Click(object sender, EventArgs e)
        {
            bool result = await DisplayAlert("Import Cards", "How are you importing the cards?", "Paste Text", "Scan Barcode");
            if (result)
            {
                string importedText = await DisplayPromptAsync("Paste QR Code Content", "Paste QR code content here");

                if (!string.IsNullOrEmpty(importedText))
                {
                    await ImportCardsFromTextAsync(importedText);
                }
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI || DeviceInfo.Platform == DevicePlatform.macOS)
            {
                await ImportCardsFromFilePickerAsync();
            } else
            {
                await ImportCardsFromBarcodeAsync();
            }
        }

        private async Task ImportCardsFromFilePickerAsync()
        {
            progressRing.IsRunning = true;
            progressRing.IsVisible = true;
            try
            {
                var fileResult = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Pick an Image with a QR Code"
                });

                if (fileResult != null)
                {
                    if (fileResult.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        fileResult.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase) ||
                        fileResult.FileName.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        using var stream = await fileResult.OpenReadAsync();
                        var barcodeReader = new BarcodeReader
                        {
                            Options = new DecodingOptions
                            {
                                PossibleFormats = new[] { ZXing.BarcodeFormat.QR_CODE }
                            }
                        };

                        var bitmap = SKBitmap.Decode(stream);
                        var decodeResult = barcodeReader.Decode(bitmap);

                        if (decodeResult != null)
                        {
                            await ImportCardsFromTextAsync(decodeResult.Text);
                        }
                        else
                        {
                            await DisplayAlert("Error", "No QR code found in the image or QR code could not be decoded.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Please select a valid image file (jpg, png, jpeg).", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Cancelled", "Image file selection cancelled.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to import cards from image file: {ex.Message}", "OK");
            }
            finally
            {
                progressRing.IsRunning = false;
                progressRing.IsVisible = false;
            }
        }

        private async Task ImportCardsFromBarcodeAsync()
        {
            progressRing.IsRunning = true;
            progressRing.IsVisible = true;
            try
            {
                var status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Denied", "Camera permission is required to scan barcodes.", "OK");
                    return;
                }

                string barcodeValue = await ShowBarcodeScannerDialog();

                if (!string.IsNullOrEmpty(barcodeValue))
                {
                    await ImportCardsFromTextAsync(barcodeValue);
                }
                else
                {
                    await DisplayAlert("Scan Cancelled", "Barcode scanning cancelled or no barcode detected.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to scan barcode: {ex.Message}", "OK");
            }
            finally
            {
                progressRing.IsRunning = false;
                progressRing.IsVisible = false;
            }
        }

        private async Task<string> ShowBarcodeScannerDialog()
        {
            var barcodeDetectedTCS = new TaskCompletionSource<string>();
            bool barcodeScanCompleted = false;

            var scanInstructionLabel = new Label
            {
                Text = "Scan QR Code to Import Cards",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var cameraBarcodeReaderView = new CameraBarcodeReaderView
            {
                Options = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormats.TwoDimensional,
                    AutoRotate = true,
                    Multiple = false
                },
                HeightRequest = 300,
                WidthRequest = 300,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 10, 0, 0)
            };

            cancelButton.Clicked += async (s, e) =>
            {
                if (!barcodeScanCompleted)
                {
                    barcodeScanCompleted = true;
                    barcodeDetectedTCS.SetResult(null);
                    await Navigation.PopModalAsync();
                }
            };

            cameraBarcodeReaderView.BarcodesDetected += (s, e) =>
            {
                if (e.Results.Length > 0)
                {
                    if (!barcodeScanCompleted)
                    {
                        barcodeScanCompleted = true;
                        barcodeDetectedTCS.SetResult(e.Results[0].Value);
                        MainThread.BeginInvokeOnMainThread(async () => { await Navigation.PopModalAsync(); });
                    }
                }
            };

            var content = new StackLayout
            {
                Children = { scanInstructionLabel, cameraBarcodeReaderView, cancelButton },
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                WidthRequest = 300
            };

            var qrCodeDialog = new ContentPage
            {
                Content = content
            };

            await Navigation.PushModalAsync(qrCodeDialog);
            return await barcodeDetectedTCS.Task;
        }

        private async Task ImportCardsFromTextAsync(string importedText)
        {
            progressRing.IsRunning = true;
            progressRing.IsVisible = true;
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

                    if (invalidCards.Count > 0)
                    {
                        string invalidCardsMessage = "The following cards are invalid and have not been added:\n" +
                                                     string.Join("\n", invalidCards.Select(c => c.CardName));
                        await DisplayAlert("Warning", invalidCardsMessage, "OK");
                    }
                    else
                    {
                        await DisplayAlert("Success", "Cards imported successfully!", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Failed to import cards. Invalid data.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to import cards: {ex.Message}", "OK");
            }
            finally
            {
                progressRing.IsRunning = false;
                progressRing.IsVisible = false;
            }
        }

        private async void ExportCards_Click(object sender, EventArgs e)
        {
            var cards = CardRepository.Instance.Cards;
            ExportedResult = _importExport.ExportCardsToText(cards);

            await ShowQrCodeDialog(ExportedResult);
        }

        private async Task ShowQrCodeDialog(string qrCodeValue)
        {
            var savingLabel = new Label
            {
                Text = "Exported cards",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var qrCodeView = new ZXing.Net.Maui.Controls.BarcodeGeneratorView
            {
                Value = qrCodeValue,
                Format = BarcodeFormat.QrCode,
                HeightRequest = 300,
                WidthRequest = 300,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var closeButton = new Button
            {
                Text = "Close",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 10, 0, 0)
            };

            closeButton.Clicked += async (s, e) =>
            {
                await Navigation.PopModalAsync();
            };

            var content = new StackLayout
            {
                Children = { savingLabel, qrCodeView, closeButton },
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                WidthRequest = 300
            };

            var qrCodeDialog = new ContentPage
            {
                Content = content
            };

            await Navigation.PushModalAsync(qrCodeDialog);
        }
    


    private async void AboutButton_Click(object sender, EventArgs e)
        {
            string aboutContent = 
            @"CardBox Tool
Version 3.0.2.0 (MAUI)
Copyright © 2025 MegaBytesMe

Source code available on GitHub:
https://github.com/megabytesme/CardBox

Anything wrong? Let us know:
https://github.com/megabytesme/CardBox/issues

Privacy Policy:
https://github.com/megabytesme/CardBox/blob/master/PRIVACYPOLICY.md

Like what you see? View my GitHub:
https://github.com/megabytesme

And maybe my Other Apps:
https://apps.microsoft.com/search?query=megabytesme

CardBox is designed to help you manage your loyalty cards effortlessly.";

            await DisplayAlert("About CardBox Tool", aboutContent, "OK");
        }
    }
}