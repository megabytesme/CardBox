using System.ComponentModel;
using SQLite;
using ZXing;

namespace Shared_Code
{
    public class Card : INotifyPropertyChanged
    {
        private long _id;
        private string _cardName;
        private string _cardNickname;
        private string _cardNumber;
        private BarcodeFormat _displayType;

        [PrimaryKey, AutoIncrement]
        public long Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string CardName
        {
            get => _cardName;
            set
            {
                if (_cardName != value)
                {
                    _cardName = value;
                    OnPropertyChanged(nameof(CardName));
                }
            }
        }

        public string CardNickname
        {
            get => _cardNickname;
            set
            {
                if (_cardNickname != value)
                {
                    _cardNickname = value;
                    OnPropertyChanged(nameof(CardNickname));
                }
            }
        }

        public string CardNumber
        {
            get => _cardNumber;
            set
            {
                if (_cardNumber != value)
                {
                    _cardNumber = value;
                    OnPropertyChanged(nameof(CardNumber));
                }
            }
        }

        public BarcodeFormat DisplayType
        {
            get => _displayType;
            set
            {
                if (_displayType != value)
                {
                    _displayType = value;
                    OnPropertyChanged(nameof(DisplayType));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
