using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Shared_Code;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;
using Windows.UI;
using Microsoft.UI.Xaml.Media;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Media;

namespace _1809_UWP
{
    public sealed partial class CardDetailPage : Page
    {
        public CardDetailPage()
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
                this.Background = new Microsoft.UI.Xaml.Media.AcrylicBrush
                {
                    BackgroundSource = Microsoft.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop,
                    TintColor = Colors.Transparent,
                    TintOpacity = 0.6,
                    FallbackColor = Colors.Gray
                };
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Card selectedCard)
            {
                this.DataContext = selectedCard;

                var barcodeBitmap = GenerateBarcode(selectedCard.CardNumber.ToString(), selectedCard.DisplayType);
                barcodeImage.Source = barcodeBitmap;

                loadingProgressRing.Visibility = Visibility.Visible;
                loadingProgressRing.IsActive = true;

                var currentLocation = await GetCurrentLocationAsync();

                if (currentLocation != null)
                {
                    var locations = await LocationService.GetNearbyLocationsAsync(
                        currentLocation.Position.Latitude,
                        currentLocation.Position.Longitude,
                        selectedCard.CardName);

                    if (locations != null && locations.Count > 0)
                    {
                        locationsListView.ItemsSource = locations;
                        noLocationsTextBlock.Visibility = Visibility.Collapsed;
                        locationErrorTextBlock.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        locationsListView.ItemsSource = null;
                        noLocationsTextBlock.Visibility = Visibility.Visible;
                        locationErrorTextBlock.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    locationsListView.ItemsSource = null;
                    noLocationsTextBlock.Visibility = Visibility.Collapsed;
                    locationErrorTextBlock.Visibility = Visibility.Visible;
                }

                loadingProgressRing.Visibility = Visibility.Collapsed;
                loadingProgressRing.IsActive = false;
            }
        }

        private WriteableBitmap GenerateBarcode(string value, DisplayType displayType)
        {
            BarcodeWriterPixelData writer = new BarcodeWriterPixelData
            {
                Format = displayType == DisplayType.Bar128 ? BarcodeFormat.CODE_128 : BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Height = 200,
                    Width = 200,
                    Margin = 1
                }
            };

            var pixelData = writer.Write(value);
            WriteableBitmap bitmap = new WriteableBitmap(pixelData.Width, pixelData.Height);

            using (var stream = bitmap.PixelBuffer.AsStream())
            {
                stream.Write(pixelData.Pixels, 0, pixelData.Pixels.Length);
            }

            return bitmap;
        }

        private void EditCard_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Card selectedCard)
            {
                Frame.Navigate(typeof(EditCardPage), selectedCard);
            }
        }

        private async void DeleteCard_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Delete Card",
                Content = "Are you sure you want to delete this card?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary && DataContext is Card selectedCard)
            {
                CardRepository.Instance.DeleteCard(selectedCard);
                Frame.GoBack();
            }
        }

        private async void NavigateToLocation_Click(object sender, RoutedEventArgs e)
        {
            var location = (sender as FrameworkElement).DataContext as LocationService.Location;
            if (location != null)
            {
                var uri = new Uri($"ms-drive-to:?destination.latitude={location.Latitude}&destination.longitude={location.Longitude}");
                await Windows.System.Launcher.LaunchUriAsync(uri);
            }
        }

        private async Task<Geopoint> GetCurrentLocationAsync()
        {
            try
            {
                var accessStatus = await Geolocator.RequestAccessAsync();

                if (accessStatus == GeolocationAccessStatus.Allowed)
                {
                    Geolocator geolocator = new Geolocator();
                    Geoposition pos = await geolocator.GetGeopositionAsync();
                    return pos.Coordinate.Point;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting location: " + ex.Message);
                return null;
            }
        }
    }
}
