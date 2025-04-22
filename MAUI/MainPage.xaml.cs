using Shared_Code;
using System.Collections.ObjectModel;

namespace CardBox
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Card> FilteredCards { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            FilteredCards = new ObservableCollection<Card>();
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
            var allCards = CardRepository.Instance.Cards;
            SearchBar searchBarControl = this.FindByName<SearchBar>("searchBar");

            var currentSearchTerm = searchBarControl?.Text ?? string.Empty;
            FilterCards(currentSearchTerm);
        }


        private Command<Card> _viewCardCommand;
        public Command<Card> ViewCardCommand => _viewCardCommand ?? (_viewCardCommand = new Command<Card>(OnViewCard));

        private async void OnViewCard(Card selectedCard)
        {
            if (selectedCard == null) return;
            await Navigation.PushAsync(new CardDetailPage(selectedCard, CardRepository.Instance));
        }
        private async void AddCardButton_Click(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddCardPage());
        }

        private void FilterCards(string searchTerm)
        {
            var lowerSearchTerm = searchTerm?.ToLowerInvariant() ?? string.Empty;
            var sourceCards = CardRepository.Instance.Cards;

            var filtered = sourceCards
                .Where(c => string.IsNullOrEmpty(lowerSearchTerm)
                            || (c.CardName?.ToLowerInvariant().Contains(lowerSearchTerm) ?? false)
                            || (c.CardNickname?.ToLowerInvariant().Contains(lowerSearchTerm) ?? false))
                .ToList();

            FilteredCards.Clear();
            foreach (var card in filtered)
            {
                FilteredCards.Add(card);
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterCards(e.NewTextValue);
        }
    }
}