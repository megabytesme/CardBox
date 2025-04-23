using System;
using System.Threading.Tasks;
using Shared_Code;
using Shared_Code_UWP.Services;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;

namespace Shared_Code_UWP.BasePages
{
    public abstract class CardDetailPageBase : Page
    {
        protected Card CurrentCard { get; private set; }
        protected abstract Image BarcodeImage { get; }
        protected abstract ProgressRing LoadingProgressRing { get; }
        protected abstract ListView LocationsListView { get; }
        protected abstract TextBlock NoLocationsTextBlock { get; }
        protected abstract TextBlock LocationErrorTextBlock { get; }
        protected abstract Type EditCardPageType { get; }
        protected abstract Type MainPageType { get; }
        protected Frame PageFrame => this.Frame;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Card selectedCard)
            {
                CurrentCard = selectedCard;
                this.DataContext = CurrentCard;
                var barcodeBitmap = BarcodeUIService.GenerateBarcodeBitmap(CurrentCard.CardNumber?.ToString(), CurrentCard.DisplayType, 200, 200);
                if (BarcodeImage != null)
                {
                    BarcodeImage.Source = barcodeBitmap;
                }
                await LoadNearbyLocations();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("CardDetailPageBase: Invalid navigation parameter.");
                if (PageFrame.CanGoBack) PageFrame.GoBack();
            }
        }

        protected async Task LoadNearbyLocations()
        {
            if (CurrentCard == null || LoadingProgressRing == null || LocationsListView == null || NoLocationsTextBlock == null || LocationErrorTextBlock == null) return;

            LoadingProgressRing.Visibility = Visibility.Visible;
            LoadingProgressRing.IsActive = true;
            LocationsListView.ItemsSource = null;
            NoLocationsTextBlock.Visibility = Visibility.Collapsed;
            LocationErrorTextBlock.Visibility = Visibility.Collapsed;

            Geopoint currentLocation = null;
            string locationErrorMsg = null;
            try
            {
                currentLocation = await GetCurrentLocationAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting location: {ex.Message}");
                locationErrorMsg = $"Could not get current location: {ex.Message}";
            }

            if (currentLocation != null)
            {
                List<LocationService.Location> locations = null;
                try
                {
                    locations = await LocationService.GetNearbyLocationsAsync(
                        currentLocation.Position.Latitude,
                        currentLocation.Position.Longitude,
                        CurrentCard.CardName);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error fetching nearby locations: {ex.Message}");
                    locationErrorMsg = $"Could not fetch nearby locations: {ex.Message}";
                }

                if (locations != null && locations.Count > 0)
                {
                    LocationsListView.ItemsSource = locations;
                }
                else if (locationErrorMsg == null)
                {
                    NoLocationsTextBlock.Visibility = Visibility.Visible;
                }
            }
            else if (locationErrorMsg == null)
            {
                locationErrorMsg = "Location access denied or unavailable.";
            }

            if (locationErrorMsg != null)
            {
                LocationErrorTextBlock.Text = locationErrorMsg;
                LocationErrorTextBlock.Visibility = Visibility.Visible;
            }

            LoadingProgressRing.IsActive = false;
            LoadingProgressRing.Visibility = Visibility.Collapsed;
        }

        protected async Task<Geopoint> GetCurrentLocationAsync()
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            if (accessStatus == GeolocationAccessStatus.Allowed)
            {
                Geolocator geolocator = new Geolocator { DesiredAccuracyInMeters = 100 };
                Geoposition pos = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                );
                return pos?.Coordinate?.Point;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Location access status: {accessStatus}");
                return null;
            }
        }

        protected void EditCardButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCard != null && EditCardPageType != null)
            {
                PageFrame.Navigate(EditCardPageType, CurrentCard);
            }
        }

        protected async void DeleteCardButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentCard == null) return;
            ContentDialogResult result = await DialogService.ShowTextDialogAsync(
               context: this,
               message: "Are you sure you want to delete this card?",
               title: "Delete Card",
               primaryButtonText: "Delete",
               secondaryButtonText: "Cancel",
               closeButtonText: null
           );

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    CardRepository.Instance.DeleteCard(CurrentCard);
                    CurrentCard = null;
                    if (PageFrame.CanGoBack)
                    {
                        PageFrame.GoBack();
                    }
                    else if (MainPageType != null)
                    {
                        PageFrame.Navigate(MainPageType);
                        PageFrame.BackStack.Clear();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting card: {ex.Message}");
                    await DialogService.ShowErrorDialogAsync(this, $"Failed to delete card: {ex.Message}");
                }
            }
        }

        protected async void NavigateToLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is LocationService.Location location)
            {
                var uri = new Uri($"ms-drive-to:?destination.latitude={location.Latitude}&destination.longitude={location.Longitude}&destination.name={Uri.EscapeDataString(location.Name ?? string.Empty)}");
                try
                {
                    var success = await Windows.System.Launcher.LaunchUriAsync(uri);
                    if (!success)
                    {
                        await DialogService.ShowErrorDialogAsync(this, "Could not launch Maps app.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error launching Maps: {ex.Message}");
                    await DialogService.ShowErrorDialogAsync(this, $"Could not launch Maps: {ex.Message}");
                }
            }
        }

        protected async void Barcode_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CurrentCard == null) return;
            int displaySize = 450;
            var largeBarcodeBitmap = BarcodeUIService.GenerateBarcodeBitmap(CurrentCard.CardNumber?.ToString(), CurrentCard.DisplayType, displaySize, displaySize);
            if (largeBarcodeBitmap != null)
            {
                await DialogService.ShowImageDialogAsync(
                   context: this,
                   imageSource: largeBarcodeBitmap,
                   imageWidth: displaySize,
                   imageHeight: displaySize,
                   title: CurrentCard.CardName ?? "Barcode",
                   description: $"Card Number: {CurrentCard.CardNumber}"
                );
            }
            else
            {
                await DialogService.ShowErrorDialogAsync(this, "Could not generate barcode image.");
            }
        }
    }
}