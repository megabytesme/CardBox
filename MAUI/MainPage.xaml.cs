using Shared_Code;
using System.Collections.ObjectModel;
using System.Linq;

namespace CardBox
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;
        public ObservableCollection<Card> FilteredCards { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            FilteredCards = new ObservableCollection<Card>(Cards);
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshCardList();
        }

        private void RefreshCardList()
        {
            CardRepository.Instance.LoadCards();
            FilteredCards.Clear();
            foreach (var card in Cards)
            {
                FilteredCards.Add(card);
            }
            SearchBar searchBarControl = this.FindByName<SearchBar>("searchBar");
            if (searchBarControl != null)
            {
                OnSearchTextChanged(searchBarControl, new TextChangedEventArgs(null, searchBarControl.Text));
            }
        }

        private Command<Card> _viewCardCommand;
        public Command<Card> ViewCardCommand => _viewCardCommand ?? (_viewCardCommand = new Command<Card>(OnViewCard));

        private async void OnViewCard(Card selectedCard)
        {
            await Navigation.PushAsync(new CardDetailPage(selectedCard, CardRepository.Instance));
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTerm = e.NewTextValue?.ToLower() ?? string.Empty;
            var filteredCards = Cards.Where(c => (c.CardName?.ToLower().Contains(searchTerm) ?? false) || (c.CardNickname?.ToLower().Contains(searchTerm) ?? false));
            FilteredCards.Clear();
            foreach (var card in filteredCards)
            {
                FilteredCards.Add(card);
            }
        }
    }
}