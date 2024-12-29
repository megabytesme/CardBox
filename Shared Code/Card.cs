namespace Shared_Code
{
    public class Card
    {
        public string CardName { get; set; } = string.Empty;
        public string CardKnickname { get; set; } = string.Empty;
        public int CardNumber { get; set; } = int.MaxValue;
        public DisplayType DisplayType { get; set; }
    }

    public enum DisplayType
    {
        BarCode,
        QR,
    }
}
