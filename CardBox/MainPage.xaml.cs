using Shared_Code;
using System.Collections.ObjectModel;

namespace CardBox
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private Command<Card> _viewCardCommand;
        public Command<Card> ViewCardCommand => _viewCardCommand ?? (_viewCardCommand = new Command<Card>(OnViewCard));

        private async void OnViewCard(Card selectedCard)
        {
            await Navigation.PushAsync(new CardDetailPage(selectedCard));
        }
    }
}
