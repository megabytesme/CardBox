using SQLite;
using System.Collections.ObjectModel;
using System.Linq;

namespace Shared_Code
{
    public class CardRepository
    {
        private static CardRepository _instance;
        private readonly SQLiteConnection _database;
        public SQLiteConnection Database => _database;
        public ObservableCollection<Card> Cards { get; private set; }

        private CardRepository()
        {
            var dbService = new DatabaseService();
            _database = dbService.Connection;
            LoadCards();
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

        private void LoadCards()
        {
            var cards = _database.Table<Card>().ToList();
            Cards = new ObservableCollection<Card>(cards);
        }

        public void AddCard(Card card)
        {
            _database.Insert(card);
            Cards.Add(card);
        }

        public void EditCard(Card card)
        {
            _database.Update(card);
            var existingCard = Cards.FirstOrDefault(c => c.Id == card.Id);
            if (existingCard != null)
            {
                int index = Cards.IndexOf(existingCard);
                if (index >= 0 && index < Cards.Count)
                {
                    Cards[index] = card;
                }
            }
        }

        public void DeleteCard(Card card)
        {
            _database.Delete(card);
            Cards.Remove(card);
        }
    }
}
