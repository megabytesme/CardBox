using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Shared_Code;
using System.Linq;
using System.ComponentModel;

namespace _1703_UWP
{
    public sealed partial class HomePage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Card> AllCards => CardRepository.Instance.Cards;

        private ObservableCollection<Card> _filteredCards;
        public ObservableCollection<Card> FilteredCards
        {
            get => _filteredCards ?? AllCards;
            set
            {
                if (!ReferenceEquals(_filteredCards, value))
                {
                    _filteredCards = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilteredCards)));
                    UpdateNoCardsUI();
                }
            }
        }

        public HomePage()
        {
            this.InitializeComponent();
            FilteredCards = new ObservableCollection<Card>(AllCards);
            UpdateNoCardsUI();
        }

        private void ViewCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Card selectedCard)
            {
                this.Frame?.Navigate(typeof(CardDetailPage), selectedCard);
            }
        }

        private void UpdateNoCardsUI()
        {
            bool noCards = FilteredCards == null || !FilteredCards.Any();
            NoCardsTextBlock.Visibility = noCards ? Visibility.Visible : Visibility.Collapsed;
            AddCardButton.Visibility = noCards ? Visibility.Visible : Visibility.Collapsed;
            CardsItemsControl.Visibility = !noCards ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AddCardButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame?.Navigate(typeof(AddCardPage));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ObservableCollection<Card> filteredResult)
            {
                FilteredCards = filteredResult;
            }
            else if (FilteredCards == null || e.NavigationMode != NavigationMode.Back)
            {
                FilteredCards = new ObservableCollection<Card>(AllCards);
            }
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button && button.Tag is string tag)
            {
                switch (tag)
                {
                    case "AddCardPage":
                        this.Frame?.Navigate(typeof(AddCardPage));
                        break;
                    case "SettingsPage":
                        this.Frame?.Navigate(typeof(SettingsPage));
                        break;
                }
            }
        }
    }
}