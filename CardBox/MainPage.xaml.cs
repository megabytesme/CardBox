using System.Collections.ObjectModel;

namespace CardBox
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Card> Cards { get; set; } = new ObservableCollection<Card>();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            // dummy cards
            Cards.Add(new Card { CardTitle = "Loyalty Card 1" });
            Cards.Add(new Card { CardTitle = "Loyalty Card 2" });
        }

        private void OnBackClicked(object sender, EventArgs e)
        {
        }

        private void OnHomeClicked(object sender, EventArgs e)
        {
        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            Cards.Add(new Card { CardTitle = "New Loyalty Card" });
        }

        private void OnSettingsClicked(object sender, EventArgs e)
        {
        }
    }

    public class Card
    {
        public string CardTitle { get; set; } = string.Empty;
    }

}
