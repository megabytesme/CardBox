namespace Shared_Code
{
    public class Card
    {
        public string CardName { get; set; } = string.Empty;
        public int CardNumber { get; set; } = int.MaxValue;
        public enum DisplayType { get; set; };
    }

    public enum DisplayType
    {
        BarCode,
        QR
    }
}
