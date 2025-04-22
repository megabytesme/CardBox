using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using Shared_Code;
using BarcodeFormat = ZXing.BarcodeFormat;

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
            var supportedTypes = Enum.GetValues(typeof(BarcodeFormat))
             .Cast<BarcodeFormat>()
             .Where(dt => BarcodeHelper.IsSupportedDisplayType(dt))
             .ToList();
            picker.ItemsSource = supportedTypes;
        }

        public Command AddCard => new Command(OnAddCard);

        private async void OnAddCard()
        {
            string cardName = ((Entry)FindByName("cardNameEntry")).Text;
            string cardNickname = ((Entry)FindByName("cardNicknameEntry")).Text;
            string cardNumber = ((Entry)FindByName("cardNumberEntry")).Text;
            Picker displayPicker = (Picker)FindByName("picker");

            if (string.IsNullOrWhiteSpace(cardName) || string.IsNullOrWhiteSpace(cardNumber) || displayPicker.SelectedIndex == -1 || displayPicker.SelectedItem == null)
            {
                await DisplayAlert("Error", "Please fill all required fields and select a display type.", "OK");
                return;
            }

            BarcodeFormat selectedDisplayType = (BarcodeFormat)displayPicker.SelectedItem;

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

                (string scannedValue, BarcodeFormat? scannedFormat) = await ShowBarcodeScannerDialog();

                if (!string.IsNullOrEmpty(scannedValue) && scannedFormat.HasValue)
                {
                    cardNumberEntry.Text = scannedValue;
                    BarcodeFormat displayType = scannedFormat.Value;

                    if (BarcodeHelper.IsSupportedDisplayType(displayType))
                    {
                        picker.SelectedItem = displayType;
                    }
                    else
                    {
                        picker.SelectedIndex = -1;
                        await DisplayAlert("Info", $"Scanned format ({displayType}) is not selectable, but number captured. Try CODE_128", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Scan Finished", "No barcode detected or scan was cancelled.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to start card scan. Please try again.", "OK");
            }
            finally
            {
                _isScanning = false;
            }
        }

        private async Task<(string, BarcodeFormat?)> ShowBarcodeScannerDialog()
        {
            var barcodeDetectedTCS = new TaskCompletionSource<(string, BarcodeFormat?)>();
            bool barcodeScanCompleted = false;

            var scanInstructionLabel = new Label
            {
                Text = "Scan Barcode",
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
                    Multiple = false,
                    TryHarder = true
                },
                HeightRequest = 300,
                WidthRequest = 300,
                HorizontalOptions = LayoutOptions.Fill,
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
                    cameraBarcodeReaderView.Handler?.DisconnectHandler();
                    barcodeDetectedTCS.TrySetResult((null, null));
                    await Navigation.PopModalAsync();
                }
            };

            cameraBarcodeReaderView.BarcodesDetected += (s, e) =>
            {
                if (e.Results != null && e.Results.Length > 0 && !barcodeScanCompleted)
                {
                    var firstResult = e.Results[0];
                    if (firstResult != null && !string.IsNullOrEmpty(firstResult.Value))
                    {
                        barcodeScanCompleted = true;
                        cameraBarcodeReaderView.Handler?.DisconnectHandler();
                        barcodeDetectedTCS.TrySetResult((firstResult.Value, (BarcodeFormat?)firstResult.Format));
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            if (Navigation.ModalStack.Count > 0)
                            {
                                await Navigation.PopModalAsync();
                            }
                        });
                    }
                }
            };

            cameraBarcodeReaderView.Loaded += (s, e) => {
                cameraBarcodeReaderView.IsDetecting = true;
            };

            var content = new StackLayout
            {
                Padding = new Thickness(20),
                Children = { scanInstructionLabel, cameraBarcodeReaderView, cancelButton },
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };

            var scannerDialogPage = new ContentPage
            {
                Content = content
            };

            scannerDialogPage.Disappearing += (s, e) => {
                cameraBarcodeReaderView.IsDetecting = false;
                cameraBarcodeReaderView.Handler?.DisconnectHandler();
                if (!barcodeScanCompleted)
                {
                    barcodeDetectedTCS.TrySetResult((null, null));
                }
            };

            await Navigation.PushModalAsync(scannerDialogPage);
            return await barcodeDetectedTCS.Task;
        }
    }
}