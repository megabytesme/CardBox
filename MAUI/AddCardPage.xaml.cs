using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using System.Collections.Generic;
using Shared_Code;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CardBox
{
    public partial class AddCardPage : ContentPage
    {
        private bool _isScanning = false;

        public AddCardPage()
        {
            InitializeComponent();
            BindingContext = this;
            PopulatePicker();
        }

        private void PopulatePicker()
        {
            var displayTypes = Enum.GetNames(typeof(DisplayType)).ToList();
            picker.ItemsSource = displayTypes;
        }

        public Command AddCard => new Command(OnAddCard);

        private async void OnAddCard()
        {
            string cardName = ((Entry)FindByName("cardNameEntry")).Text;
            string cardNickname = ((Entry)FindByName("cardNicknameEntry")).Text;
            string cardNumber = ((Entry)FindByName("cardNumberEntry")).Text;
            Picker displayPicker = (Picker)FindByName("picker");

            if (string.IsNullOrWhiteSpace(cardName) || string.IsNullOrWhiteSpace(cardNumber) || displayPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Please fill all required fields.", "OK");
                return;
            }

            if (!Enum.TryParse(displayPicker.SelectedItem.ToString(), out Shared_Code.DisplayType selectedDisplayType))
            {
                await DisplayAlert("Error", "Invalid display type selected.", "OK");
                return;
            }

            var newCard = new Card
            {
                CardName = cardName,
                CardNickname = cardNickname,
                CardNumber = cardNumber,
                DisplayType = selectedDisplayType
            };

            CardRepository.Instance.AddCard(newCard);

            await Shell.Current.GoToAsync("//MainPage");
        }

        private async void ScanCard_Click(object sender, EventArgs e)
        {
            if (_isScanning) return;
            _isScanning = true;

            try
            {
                var status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Denied", "Camera permission is required to scan card.", "OK");
                    _isScanning = false;
                    return;
                }

                (string scannedValue, string barcodeFormat) = await ShowBarcodeScannerDialog();

                if (!string.IsNullOrEmpty(scannedValue))
                {
                    cardNumberEntry.Text = scannedValue;
                    DisplayType displayType;
                    if (Enum.TryParse(barcodeFormat, out displayType))
                    {
                        picker.SelectedItem = displayType.ToString();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Unrecognized barcode format.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No barcode detected or scan was cancelled.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to start card scan: {ex.Message}", "OK");
            }
            finally
            {
                _isScanning = false;
            }
        }

        private async Task<(string, string)> ShowBarcodeScannerDialog()
        {
            var barcodeDetectedTCS = new TaskCompletionSource<(string, string)>();
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
                    Formats = BarcodeFormats.All,
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
                    barcodeDetectedTCS.SetResult((null, null));
                    await Navigation.PopModalAsync();
                }
            };

            cameraBarcodeReaderView.BarcodesDetected += (s, e) =>
            {
                if (e.Results.Length > 0 && !barcodeScanCompleted)
                {
                    barcodeScanCompleted = true;
                    barcodeDetectedTCS.SetResult((e.Results[0].Value, e.Results[0].Format.ToString()));
                    MainThread.BeginInvokeOnMainThread(async () => { await Navigation.PopModalAsync(); });
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
    }
}
