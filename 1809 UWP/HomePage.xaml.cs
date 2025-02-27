﻿using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Shared_Code;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Xaml.Media;
using System.Linq;
using muxc = Microsoft.UI.Xaml.Controls;
namespace _1809_UWP
{
    public sealed partial class HomePage : Page
    {
        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;

        private ObservableCollection<Card> filteredCards;
        public ObservableCollection<Card> FilteredCards
        {
            get => filteredCards ?? Cards;
            set
            {
                filteredCards = value;
                Bindings.Update();
                UpdateNoCardsUI();
            }
        }

        public HomePage()
        {
            this.InitializeComponent();
            DataContext = this;
            ApplyBackdropOrAcrylic();
            UpdateNoCardsUI();
        }

        private void ApplyBackdropOrAcrylic()
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 12))
            {
                muxc.BackdropMaterial.SetApplyToRootOrPageBackground(this, true);
            }
            else
            {
                this.Background = new AcrylicBrush
                {
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                    TintColor = Colors.Transparent,
                    TintOpacity = 0.6,
                    FallbackColor = Colors.Gray
                };
            }
        }

        private void ViewCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Card selectedCard)
            {
                Frame.Navigate(typeof(CardDetailPage), selectedCard);
            }
        }

        private void UpdateNoCardsUI()
        {
            bool noCards = FilteredCards == null || !FilteredCards.Any();
            NoCardsTextBlock.Visibility = noCards ? Visibility.Visible : Visibility.Collapsed;
            AddCardButton.Visibility = noCards ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AddCardButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddCardPage));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ObservableCollection<Card> filteredCards)
            {
                FilteredCards = filteredCards;
            }
            UpdateNoCardsUI();
        }
    }
}
