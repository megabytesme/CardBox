using System.ComponentModel;
using SQLite;

namespace Shared_Code
{
    public class Card : INotifyPropertyChanged
    {
        private long _id;
        private string _cardName;
        private string _cardNickname;
        private string _cardNumber;
        private DisplayType _displayType;

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

        public DisplayType DisplayType
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

    public enum DisplayType
    {
        // (Only the enum) is based on the ZXing.NET.MAUI package
        //
        // Summary:
        //     Aztec 2D barcode format.
        Aztec = 0,
        //
        // Summary:
        //     CODABAR 1D format.
        Codabar = 1,
        //
        // Summary:
        //     Code 39 1D format.
        Code39 = 2,
        //
        // Summary:
        //     Code 93 1D format.
        Code93 = 3,
        //
        // Summary:
        //     Code 128 1D format.
        Code128 = 4,
        //
        // Summary:
        //     Data Matrix 2D barcode format.
        DataMatrix = 5,
        //
        // Summary:
        //     EAN-8 1D format.
        Ean8 = 6,
        //
        // Summary:
        //     EAN-13 1D format.
        Ean13 = 7,
        //
        // Summary:
        //     ITF (Interleaved Two of Five) 1D format.
        Itf = 8,
        //
        // Summary:
        //     MaxiCode 2D barcode format.
        MaxiCode = 9,
        //
        // Summary:
        //     PDF417 format.
        Pdf417 = 10,
        //
        // Summary:
        //     QR Code 2D barcode format.
        QrCode = 11,
        //
        // Summary:
        //     RSS 14
        Rss14 = 12,
        //
        // Summary:
        //     RSS EXPANDED
        RssExpanded = 13,
        //
        // Summary:
        //     UPC-A 1D format.
        UpcA = 14,
        //
        // Summary:
        //     UPC-E 1D format.
        UpcE = 15,
        //
        // Summary:
        //     UPC/EAN extension format. Not a stand-alone format.
        UpcEanExtension = 16,
        //
        // Summary:
        //     MSI
        Msi = 17,
        //
        // Summary:
        //     Plessey
        Plessey = 18,
        //
        // Summary:
        //     Intelligent Mail barcode
        Imb = 19,
        //
        // Summary:
        //     Pharmacode format.
        PharmaCode = 20
    }
}
