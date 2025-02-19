using SQLite;

namespace Shared_Code
{
    public class Card
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string CardName { get; set; } = string.Empty;
        public string CardNickname { get; set; } = string.Empty;
        public int CardNumber { get; set; } = int.MaxValue;
        public DisplayType DisplayType { get; set; }
    }

    public enum DisplayType
    {
        BarCode,
        QR,
    }
}
