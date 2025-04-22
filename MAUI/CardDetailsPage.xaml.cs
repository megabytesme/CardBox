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

            try
            {
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
            catch (Exception ex)
            {
                barcodeImage.Source = null;
            }
        }

        private async void OnBarcodeImageTapped(object sender, TappedEventArgs e)
        {
            if (barcodeImage?.Source == null)
            {
                return;
            }

            var fullScreenImage = new Image
            {
                Source = barcodeImage.Source,
                Aspect = Aspect.AspectFit,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            var gridLayout = new Grid
            {
                BackgroundColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            gridLayout.Children.Add(fullScreenImage);

            var closeGesture = new TapGestureRecognizer();
            closeGesture.Tapped += async (s, args) =>
            {
                await Navigation.PopModalAsync();
            };
            gridLayout.GestureRecognizers.Add(closeGesture);

            var fullScreenPage = new ContentPage
            {
                Content = gridLayout,
                BackgroundColor = Colors.Black
            };

            await Navigation.PushModalAsync(fullScreenPage, animated: false);
        }

        private async void InitializePage(Card selectedCard)
        {
            loadingProgressRing.IsVisible = true;
            loadingProgressRing.IsRunning = true;
            locationErrorLabel.IsVisible = false;
            noLocationsLabel.IsVisible = false;

            try
            {
                var currentLocation = await GetCurrentLocationAsync();

                if (currentLocation != null)
                {
                    var locations = await LocationService.GetNearbyLocationsAsync(
                        currentLocation.Latitude,
                        currentLocation.Longitude,
                        selectedCard.CardName);

                    if (locations != null && locations.Any())
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
                    noLocationsLabel.IsVisible = false;
                    locationsListView.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                locationErrorLabel.Text = $"An error occurred: {ex.Message}";
                locationErrorLabel.IsVisible = true;
                noLocationsLabel.IsVisible = false;
                locationsListView.ItemsSource = null;
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
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        if (locationErrorLabel != null)
                        {
                            locationErrorLabel.Text = "Location permission denied.";
                            locationErrorLabel.IsVisible = true;
                        }
                        return null;
                    }
                }

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request, CancellationToken.None);

                return location;
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                System.Diagnostics.Debug.WriteLine($"Geolocation not supported: {fnsEx.Message}");
                if (locationErrorLabel != null)
                {
                    locationErrorLabel.Text = "Geolocation is not supported on this device.";
                    locationErrorLabel.IsVisible = true;
                }
                return null;
            }
            catch (FeatureNotEnabledException fneEx)
            {
                System.Diagnostics.Debug.WriteLine($"Geolocation not enabled: {fneEx.Message}");
                if (locationErrorLabel != null)
                {
                    locationErrorLabel.Text = "Location services are not enabled on this device.";
                    locationErrorLabel.IsVisible = true;
                }
                return null;
            }
            catch (PermissionException pEx)
            {
                System.Diagnostics.Debug.WriteLine($"Geolocation permission error: {pEx.Message}");
                if (locationErrorLabel != null)
                {
                    locationErrorLabel.Text = "Location permission not granted.";
                    locationErrorLabel.IsVisible = true;
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting location: {ex.Message}");
                if (locationErrorLabel != null)
                {
                    locationErrorLabel.Text = "Unable to retrieve location.";
                    locationErrorLabel.IsVisible = true;
                }
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
                    var locationToOpen = new Microsoft.Maui.Devices.Sensors.Location(selectedLocation.Latitude, selectedLocation.Longitude);
                    await Map.Default.OpenAsync(locationToOpen, options);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Map Error: {ex}");
                    await DisplayAlert("Error", "Could not open map application. Please ensure a map app is installed.", "OK");
                }
            }
        }
    }
}