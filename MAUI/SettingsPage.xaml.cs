using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Shared_Code;
using ZXing.Net.Maui;
using ZXing.Common;
using ZXing.Rendering;
using Microsoft.Maui;
using System.IO;
using ZXing.Net;
using BarcodeFormat = ZXing.Net.Maui.BarcodeFormat;

namespace CardBox
{
    public partial class SettingsPage : ContentPage
    {
        private readonly CardImportExport _importExport = new CardImportExport();

        public SettingsPage()
        {
            InitializeComponent();
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
            string action = await DisplayActionSheet("Import Cards", "Cancel", null, "Paste Text", "Scan Barcode");

            if (action == "Paste Text")
            {
                string importedText = await DisplayPromptAsync("Paste QR Code Content", "Paste QR code content here");

                if (!string.IsNullOrEmpty(importedText))
                {
                    await ImportCardsFromTextAsync(importedText);
                }
            }
            else if (action == "Scan Barcode")
            {
                await ImportCardsFromBarcodeAsync();
            }
        }

        private async Task ImportCardsFromBarcodeAsync()
        {

        }

        private async Task ImportCardsFromTextAsync(string importedText)
        {
            progressRing.IsRunning = true;
            progressRing.IsVisible = true;
            try
            {
                var importedCards = _importExport.ImportCardsFromTextAsync(importedText);

                if (importedCards != null)
                {
                    foreach (var card in importedCards)
                    {
                        CardRepository.Instance.Cards.Add(card);
                        CardRepository.Instance.Database.Insert(card);
                    }

                    await DisplayAlert("Success", "Cards imported successfully!", "OK");
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
            progressRing.IsRunning = true;
            progressRing.IsVisible = true;

            var cards = CardRepository.Instance.Cards;
            string exportedText = _importExport.ExportCardsToText(cards);

            progressRing.IsRunning = false;
            progressRing.IsVisible = false;

        }

        private async Task ShowQrCodeDialog(ImageSource qrCodeBitmap)
        {
            var image = new Image
            {
                Source = qrCodeBitmap,
                Aspect = Aspect.AspectFit,
                WidthRequest = 200,
                HeightRequest = 200
            };

            await DisplayAlert("Exported QR Code", "", "OK"); // Simple alert dialog to show the QR Code
            //For a custom dialog you need to dive into custom renderers/handlers
        }
    }
}
