using System.Collections.ObjectModel;

namespace Shared_Code
{
    public class CardRepository
    {
        private static CardRepository _instance;
        public ObservableCollection<Card> Cards { get; private set; } = new ObservableCollection<Card>();

        private CardRepository()
        {
            Cards.Add(new Card { CardName = "Loyalty Card 1" });
            Cards.Add(new Card { CardName = "Loyalty Card 2" });
        }

        public static CardRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CardRepository();
                }
                return _instance;
            }
        }

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }
    }
}
