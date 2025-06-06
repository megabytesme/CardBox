﻿using Shared_Code;
using ZXing.Net.Maui;
using ZXing.Common;
using BarcodeFormat = ZXing.Net.Maui.BarcodeFormat;
using ZXing.Net.Maui.Controls;
using SkiaSharp;
using ZXing.SkiaSharp;

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
            var aboutDialog = new ContentPage();
            var grid = new Grid
            {
                RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
                Padding = new Thickness(15),
                BackgroundColor = AppInfo.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#FF202020")
                    : Colors.White
            };

            var titleLabel = new Label
            {
                Text = "About CardBox Tool",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10),
                TextColor = AppInfo.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black
            };
            Grid.SetRow(titleLabel, 0);
            grid.Children.Add(titleLabel);

            var scrollView = new ScrollView();
            Grid.SetRow(scrollView, 1);
            grid.Children.Add(scrollView);

            var contentLabel = new Label
            {
                TextColor = AppInfo.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black
            };
            var formattedString = new FormattedString();

            void AddHyperlinkSpan(string text, string url)
            {
                var span = new Span
                {
                    Text = text,
                    TextDecorations = TextDecorations.Underline,
                    TextColor = AppInfo.RequestedTheme == AppTheme.Dark ? Colors.LightSkyBlue : Colors.Blue
                };
                span.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () => await OpenBrowser(url))
                });
                formattedString.Spans.Add(span);
            }

            formattedString.Spans.Add(new Span { Text = "CardBox Tool\n" });
            formattedString.Spans.Add(new Span { Text = "Version 3.0.2.0 (MAUI)\n" });
            formattedString.Spans.Add(new Span { Text = "Copyright © 2025 MegaBytesMe\n\n" });

            formattedString.Spans.Add(new Span { Text = "Source code available on " });
            AddHyperlinkSpan("GitHub", "https://github.com/megabytesme/CardBox");

            formattedString.Spans.Add(new Span { Text = "\nAnything wrong? Let us know: " });
            AddHyperlinkSpan("Support", "https://github.com/megabytesme/CardBox/issues");

            formattedString.Spans.Add(new Span { Text = "\nPrivacy Policy: " });
            AddHyperlinkSpan("Privacy Policy", "https://github.com/megabytesme/CardBox/blob/master/PRIVACYPOLICY.md");

            formattedString.Spans.Add(new Span { Text = "\nLike what you see? View my " });
            AddHyperlinkSpan("GitHub", "https://github.com/megabytesme");
            formattedString.Spans.Add(new Span { Text = " and maybe my " });
            AddHyperlinkSpan("Other Apps", "https://apps.microsoft.com/search?query=megabytesme");

            formattedString.Spans.Add(new Span { Text = "\n\nCardBox is designed to help you manage your loyalty cards effortlessly." });

            contentLabel.FormattedText = formattedString;

            scrollView.Content = contentLabel;

            var closeButton = new Button
            {
                Text = "OK",
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalOptions = LayoutOptions.Center,
                BackgroundColor = AppInfo.RequestedTheme == AppTheme.Dark ? Colors.DarkSlateGray : Colors.LightGray,
                TextColor = AppInfo.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black
            };
            Grid.SetRow(closeButton, 2);
            grid.Children.Add(closeButton);

            closeButton.Clicked += async (s, args) =>
            {
                await Navigation.PopModalAsync();
            };

            aboutDialog.Content = grid;
            await Navigation.PushModalAsync(aboutDialog);
        }

        private async Task OpenBrowser(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not open link: {ex.Message}", "OK");
            }
        }
    }
}