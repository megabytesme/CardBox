using System.Collections.ObjectModel;
using Shared_Code;
using ZXing.Net.Maui;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;

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

            barcodeImage.Format = selectedCard.DisplayType switch
            {
                DisplayType.Code128 => BarcodeFormat.Code128,
                DisplayType.QrCode => BarcodeFormat.QrCode,
                DisplayType.Aztec => BarcodeFormat.Aztec,
                DisplayType.Codabar => BarcodeFormat.Codabar,
                DisplayType.Code39 => BarcodeFormat.Code39,
                DisplayType.Code93 => BarcodeFormat.Code93,
                DisplayType.DataMatrix => BarcodeFormat.DataMatrix,
                DisplayType.Ean8 => BarcodeFormat.Ean8,
                DisplayType.Ean13 => BarcodeFormat.Ean13,
                DisplayType.Itf => BarcodeFormat.Itf,
                DisplayType.MaxiCode => BarcodeFormat.MaxiCode,
                DisplayType.Pdf417 => BarcodeFormat.Pdf417,
                DisplayType.Rss14 => BarcodeFormat.Rss14,
                DisplayType.RssExpanded => BarcodeFormat.RssExpanded,
                DisplayType.UpcA => BarcodeFormat.UpcA,
                DisplayType.UpcE => BarcodeFormat.UpcE,
                DisplayType.UpcEanExtension => BarcodeFormat.UpcEanExtension,
                DisplayType.Msi => BarcodeFormat.Msi,
                DisplayType.Plessey => BarcodeFormat.Plessey,
                DisplayType.Imb => BarcodeFormat.Imb,
                DisplayType.PharmaCode => BarcodeFormat.PharmaCode,
                _ => BarcodeFormat.QrCode
            };

            InitializePage(selectedCard);
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
                await Shell.Current.GoToAsync($"editCard?CardId={selectedCard.Id}");
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
                        await Shell.Current.GoToAsync("..");
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
            if (BindingContext is LocationService.Location selectedLocation)
            {
                string uri = $"geo:{selectedLocation.Latitude},{selectedLocation.Longitude}";

                if (await Launcher.CanOpenAsync(new Uri(uri)))
                {
                    try
                    {
                        await Launcher.OpenAsync(new Uri(uri));
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", "Could not open maps: " + ex.Message, "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No maps app found.", "OK");
                }
            }
        }
    }
}
