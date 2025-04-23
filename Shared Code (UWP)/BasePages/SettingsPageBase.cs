using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared_Code;
using Shared_Code_UWP.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace Shared_Code_UWP.BasePages
{
    public abstract class SettingsPageBase : Page
    {
        protected readonly CardImportExport _importExport = new CardImportExport();
        protected abstract ProgressRing LoadingProgressRing { get; }
        protected abstract Type ScannerPageType { get; }
        protected abstract string AppVersionString { get; }
        protected abstract AddCardPageBase.IScannerResult GetLastScanResult();
        protected abstract void ClearLastScanResult();
        protected Frame PageFrame => this.Frame;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                var lastScanResult = GetLastScanResult();
                if (lastScanResult != null)
                {
                    await ImportCardsFromBarcodeResultAsync(lastScanResult);
                    ClearLastScanResult();
                }
            }
        }

        private async Task ImportCardsFromBarcodeResultAsync(AddCardPageBase.IScannerResult result)
        {
            if (result != null && !string.IsNullOrEmpty(result.Text))
            {
                await ImportCardsFromTextAsync(result.Text);
            }
            else
            {
                await DialogService.ShowErrorDialogAsync(this, "Barcode scan failed, was cancelled, or contained no data.");
            }
        }

        protected async void ResetAppButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await DialogService.ShowTextDialogAsync(
               context: this,
               message: "Are you sure you want to reset the app? This will delete all cards and cannot be undone.",
               title: "Confirm Reset",
               primaryButtonText: "Reset",
               secondaryButtonText: "Cancel",
               closeButtonText: null
            );

            if (result == ContentDialogResult.Primary && LoadingProgressRing != null)
            {
                LoadingProgressRing.IsActive = true;
                try
                {
                    CardRepository.Instance.Cards.Clear();
                    await Task.Run(() => CardRepository.Instance.Database.DeleteAll<Card>());
                    await DialogService.ShowSuccessDialogAsync(this, "App has been reset.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error resetting app: {ex.Message}");
                    await DialogService.ShowErrorDialogAsync(this, $"Failed to reset app: {ex.Message}");
                    try { CardRepository.Instance.LoadCards(); } catch { }
                }
                finally
                {
                    LoadingProgressRing.IsActive = false;
                }
            }
        }

        protected async void ImportCardsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Import Cards",
                PrimaryButtonText = "Paste Text",
                SecondaryButtonText = "Scan QR Code",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                RequestedTheme = this.RequestedTheme
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await PromptForPastedTextAndImportAsync();
            }
            else if (result == ContentDialogResult.Secondary)
            {
                Type scannerPageType = ScannerPageType;
                if (scannerPageType != null)
                {
                    PageFrame.Navigate(scannerPageType);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ScannerPage type not specified in derived class.");
                    await DialogService.ShowErrorDialogAsync(this, "Cannot navigate to scanner.");
                }
            }
        }

        private async Task PromptForPastedTextAndImportAsync()
        {
            var textBox = new TextBox
            {
                PlaceholderText = "Paste exported text (usually from a QR code) here",
                AcceptsReturn = true,
                Height = 150,
                TextWrapping = TextWrapping.Wrap
            };
            textBox.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);


            var inputDialog = new ContentDialog
            {
                Title = "Paste Card Data",
                Content = textBox,
                PrimaryButtonText = "Import",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                RequestedTheme = this.RequestedTheme
            };

            var inputResult = await inputDialog.ShowAsync();

            if (inputResult == ContentDialogResult.Primary)
            {
                string importedText = textBox.Text;
                if (!string.IsNullOrWhiteSpace(importedText))
                {
                    await ImportCardsFromTextAsync(importedText);
                }
                else
                {
                    await DialogService.ShowWarningDialogAsync(this, "No text pasted to import.");
                }
            }
        }

        protected async Task ImportCardsFromTextAsync(string importedText)
        {
            if (string.IsNullOrWhiteSpace(importedText))
            {
                await DialogService.ShowWarningDialogAsync(this, "Import data is empty.");
                return;
            }
            if (LoadingProgressRing == null) return;

            LoadingProgressRing.IsActive = true;
            ObservableCollection<Card> importedCards = null;
            List<Card> invalidCards = null;
            bool importError = false;
            string importErrorMessage = "Failed to import cards. Unknown error.";

            try
            {
                await Task.Run(() =>
                {
                    importedCards = _importExport.ImportCardsFromTextAsync(importedText, out invalidCards);
                });

                if (importedCards != null)
                {
                    int addedCount = 0;
                    if (importedCards.Any())
                    {
                        foreach (var card in importedCards)
                        {
                            CardRepository.Instance.AddCard(card);
                            addedCount++;
                        }
                    }

                    LoadingProgressRing.IsActive = false;
                    string message = "";
                    if (addedCount > 0) message += $"{addedCount} card{(addedCount > 1 ? "s" : "")} imported successfully.\n\n";

                    if (invalidCards != null && invalidCards.Count > 0)
                    {
                        message += $"The following {invalidCards.Count} item{(invalidCards.Count > 1 ? "s" : "")} had invalid data or format and could not be added:\n" +
                                   string.Join("\n", invalidCards.Select(c => $"- {c.CardName ?? "Unnamed Card"} ({c.CardNumber ?? "No Number"})"));
                        if (addedCount > 0) await DialogService.ShowWarningDialogAsync(this, message, "Import Partially Completed");
                        else await DialogService.ShowErrorDialogAsync(this, message, "Import Failed");
                    }
                    else if (addedCount > 0)
                    {
                        await DialogService.ShowSuccessDialogAsync(this, message.Trim(), "Import Complete");
                    }
                    else
                    {
                        await DialogService.ShowWarningDialogAsync(this, "No valid cards found in the provided data.", "Import Empty");
                    }
                }
                else
                {
                    importError = true;
                    importErrorMessage = "Failed to import cards. The provided data might be corrupt or in an unrecognized format.";
                }
            }
            catch (Exception ex)
            {
                importError = true;
                importErrorMessage = $"An error occurred during import: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"ImportCardsFromTextAsync Error: {ex}");
            }
            finally
            {
                LoadingProgressRing.IsActive = false;
                if (importError)
                {
                    await DialogService.ShowErrorDialogAsync(this, importErrorMessage);
                }
            }
        }

        protected async void ExportCardsButton_Click(object sender, RoutedEventArgs e)
        {
            var cards = CardRepository.Instance.Cards;
            if (cards == null || !cards.Any())
            {
                await DialogService.ShowWarningDialogAsync(this, "There are no cards to export.");
                return;
            }
            if (LoadingProgressRing == null) return;

            LoadingProgressRing.IsActive = true;
            string exportedText = null;
            WriteableBitmap qrCodeBitmap = null;
            bool exportError = false;
            string exportErrorMessage = "Failed to export cards.";

            try
            {
                await Task.Run(() =>
                {
                    exportedText = _importExport.ExportCardsToText(cards);
                });

                if (string.IsNullOrEmpty(exportedText))
                {
                    exportError = true;
                    exportErrorMessage = "Failed to generate export data.";
                }
                else
                {
                    qrCodeBitmap = await BarcodeUIService.GenerateQrCodeBitmapAsync(exportedText);
                    if (qrCodeBitmap == null)
                    {
                        exportError = true;
                        exportErrorMessage = "Failed to generate QR code image.";
                    }
                }
            }
            catch (Exception ex)
            {
                exportError = true;
                exportErrorMessage = $"An error occurred during export: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"ExportCardsButton_Click Error: {ex}");
            }
            finally
            {
                LoadingProgressRing.IsActive = false;
            }

            if (exportError)
            {
                await DialogService.ShowErrorDialogAsync(this, exportErrorMessage);
            }
            else if (qrCodeBitmap != null && !string.IsNullOrEmpty(exportedText))
            {
                int qrDisplaySize = 400;
                ContentDialogResult result = await DialogService.ShowImageDialogAsync(
                    context: this,
                    imageSource: qrCodeBitmap,
                    imageWidth: qrDisplaySize,
                    imageHeight: qrDisplaySize,
                    title: "Exported QR Code",
                    description: "Scan this code to import cards on another device.\n(Tap image or button to copy raw text data).",
                    primaryButtonText: "Copy Text Data"
                );

                if (result == ContentDialogResult.Primary)
                {
                    CopyExportedTextToClipboard(exportedText);
                    await DialogService.ShowSuccessDialogAsync(this, "Exported text copied to clipboard.");
                }
            }
        }

        protected async void CopyExportedTextToClipboard(string exportedText)
        {
            try
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(exportedText);
                Clipboard.SetContent(dataPackage);
                Clipboard.Flush();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying to clipboard: {ex.Message}");
                await DialogService.ShowErrorDialogAsync(this, $"Could not copy text to clipboard: {ex.Message}");
            }
        }

        protected async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            string version = AppVersionString ?? "Unknown Version";
            TextBlock aboutContent = new TextBlock()
            {
                Inlines =
                {
                    new Run() { Text = "CardBox Tool", FontWeight = Windows.UI.Text.FontWeights.SemiBold }, new LineBreak(),
                    new Run() { Text = $"Version {version}" }, new LineBreak(),
                    new Run() { Text = "Copyright © 2025 MegaBytesMe" }, new LineBreak(),
                    new Run() { Text = " "}, new LineBreak(),
                    new Run() { Text = "Source code available on " }, CreateHyperlink("https://github.com/megabytesme/CardBox", "GitHub"), new LineBreak(),
                    new Run() { Text = "Found an issue? Report it: " }, CreateHyperlink("https://github.com/megabytesme/CardBox/issues", "Support"), new LineBreak(),
                    new Run() { Text = "View the " }, CreateHyperlink("https://github.com/megabytesme/CardBox/blob/master/PRIVACYPOLICY.md", "Privacy Policy"), new LineBreak(),
                    new Run() { Text = " "}, new LineBreak(),
                    new Run() { Text = "More by MegaBytesMe: " }, CreateHyperlink("https://github.com/megabytesme", "GitHub Profile"), new Run() { Text = " | " }, CreateHyperlink("https://apps.microsoft.com/search?query=megabytesme", "Microsoft Store"), new LineBreak(),
                    new Run() { Text = " "}, new LineBreak(),
                    new Run() { Text = "CardBox is designed to help you manage your loyalty cards effortlessly." }
                 },
                TextWrapping = TextWrapping.Wrap,
                IsTextSelectionEnabled = true
            };

            ScrollViewer scrollableContent = new ScrollViewer()
            {
                Content = aboutContent,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            await DialogService.ShowContentDialogAsync(
                context: this,
                contentElement: scrollableContent,
                title: "About CardBox Tool",
                closeButtonText: "OK"
            );
        }

        private Hyperlink CreateHyperlink(string url, string text)
        {
            var link = new Hyperlink { NavigateUri = new Uri(url) };
            link.Inlines.Add(new Run { Text = text });
            ToolTipService.SetToolTip(link, url);
            link.Click += async (s, e) => {
                try { await Windows.System.Launcher.LaunchUriAsync(link.NavigateUri); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Failed to launch URI {link.NavigateUri}: {ex.Message}"); }
            };
            return link;
        }
    }
}