using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Shared_Code;
using SkiaSharp;

namespace CardBox
{
    public partial class CardDetailPage : ContentPage
    {
        private readonly CardRepository _cardRepository;

        public CardDetailPage(Card selectedCard, CardRepository cardRepository)
        {
            InitializeComponent();
            BindingContext = selectedCard;
            _cardRepository = cardRepository;
            InitializePage(selectedCard);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            GenerateBarcode();
        }

        private void GenerateBarcode()
        {
            if (barcodeImage == null || BindingContext is not Card selectedCard)
                return;

            ZXing.BarcodeWriterPixelData writer = new ZXing.BarcodeWriterPixelData
            {
                Format = selectedCard.DisplayType,
                Options = new ZXing.Common.EncodingOptions
                {
                    Height = 200,
                    Width = 200,
                    Margin = 1
                }
            };

            var pixelData = writer.Write(selectedCard.CardNumber);
            SKImageInfo imageInfo = new SKImageInfo(pixelData.Width, pixelData.Height, SKColorType.Rgba8888);

            using var bitmap = new SKBitmap(imageInfo);
            IntPtr pixelPtr = Marshal.UnsafeAddrOfPinnedArrayElement(pixelData.Pixels, 0);
            bitmap.InstallPixels(imageInfo, pixelPtr);

            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                var stream = new MemoryStream();
                data.SaveTo(stream);
                stream.Seek(0, SeekOrigin.Begin);

                barcodeImage.Source = ImageSource.FromStream(() => new MemoryStream(stream.ToArray()));
            }
        }

        private async void InitializePage(Card selectedCard)
        {
            loadingProgressRing.IsVisible = true;
            loadingProgressRing.IsRunning = true;
            locationErrorLabel.IsVisible = false;

            try
            {
                var currentLocation = await GetCurrentLocationAsync();

                if (currentLocation != null)
                {
                    var locations = await LocationService.GetNearbyLocationsAsync(
                        currentLocation.Latitude,
                        currentLocation.Longitude,
                        selectedCard.CardName);

                    if (locations != null && locations.Count > 0)
                    {
                        locationsListView.ItemsSource = new ObservableCollection<LocationService.Location>(locations);
                        noLocationsLabel.IsVisible = false;
                    }
                    else
                    {
                        locationsListView.ItemsSource = null;
                        noLocationsLabel.IsVisible = true;
                    }
                }
                else
                {
                    locationErrorLabel.Text = "Could not retrieve location. Please try again later.";
                    locationErrorLabel.IsVisible = true;
                    noLocationsLabel.IsVisible = false;
                    locationsListView.ItemsSource = null;
                }
            }
            finally
            {
                loadingProgressRing.IsVisible = false;
                loadingProgressRing.IsRunning = false;
            }
        }

        private async Task<Microsoft.Maui.Devices.Sensors.Location> GetCurrentLocationAsync()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location != null)
                {
                    return location;
                }

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                location = await Geolocation.GetLocationAsync(request);

                return location;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting location: " + ex.Message);
                return null;
            }
        }

        private async void EditCard_Click(object sender, EventArgs e)
        {
            if (BindingContext is Card selectedCard)
            {
                await Navigation.PushAsync(new EditCardPage(selectedCard));
            }
        }

        private async void DeleteCard_Click(object sender, EventArgs e)
        {
            if (BindingContext is Card selectedCard)
            {
                bool result = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete {selectedCard.CardName}?", "Yes", "No");

                if (result)
                {
                    try
                    {
                        _cardRepository.DeleteCard(selectedCard);
                        await Navigation.PopAsync();
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", "Error deleting card: " + ex.Message, "OK");
                    }
                }
            }
        }

        private async void NavigateToLocation_Click(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is LocationService.Location selectedLocation)
            {
                var options = new MapLaunchOptions
                {
                    Name = selectedLocation.Name,
                    NavigationMode = NavigationMode.Driving
                };

                try
                {
                    await Map.Default.OpenAsync(new Location(selectedLocation.Latitude, selectedLocation.Longitude), options);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", "No map application available to open.", "OK");
                }
            }
        }
    }
}