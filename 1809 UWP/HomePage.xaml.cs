using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Shared_Code;
using Windows.UI.Xaml.Media;
using System.Linq;
using System.ComponentModel;

namespace _1809_UWP
{
    public sealed partial class HomePage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;

        private ObservableCollection<Card> _filteredCards;
        public ObservableCollection<Card> FilteredCards
        {
            get => _filteredCards ?? Cards;
            set
            {
                if (_filteredCards != value)
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
            MaterialHelper.ApplySystemBackdropOrAcrylic(this);
            UpdateNoCardsUI();
        }

        private void ViewCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Card selectedCard)
            {
                var mainPage = FindParent<MainPage>(this);
                var frame = mainPage?.MainContentFrame ?? this.Frame;
                frame?.Navigate(typeof(CardDetailPage), selectedCard);
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
            var mainPage = FindParent<MainPage>(this);
            var frame = mainPage?.MainContentFrame ?? this.Frame;
            frame?.Navigate(typeof(AddCardPage));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ObservableCollection<Card> filteredResult)
            {
                FilteredCards = filteredResult;
            }
            else
            {
                FilteredCards = Cards;
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null) return parent;
            else return FindParent<T>(parentObject);
        }
    }
}