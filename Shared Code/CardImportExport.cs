using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using ZXing;

namespace Shared_Code
{
    public class CardImportExport
    {
        public ObservableCollection<Card> ImportCardsFromTextAsync(string importedText, out List<Card> invalidCards)
        {
            invalidCards = new List<Card>();

            try
            {
                string decompressedJson = DecompressJson(importedText);
                var importedDictionaries = DeserializeCardDictionaries(decompressedJson);
                var importedCards = new ObservableCollection<Card>();

                if (importedDictionaries != null)
                {
                    long nextId = 1;
                    foreach (var dict in importedDictionaries)
                    {
                        var card = new Card
                        {
                            Id = nextId++,
                            CardName = dict.ContainsKey("n") ? (string)dict["n"] : string.Empty,
                            CardNickname = dict.ContainsKey("k") ? (string)dict["k"] : string.Empty,
                            CardNumber = dict.ContainsKey("c") ? (string)dict["c"] : string.Empty,
                            DisplayType = dict.ContainsKey("t") && Enum.TryParse(dict["t"].ToString(), out BarcodeFormat format)
                                         ? format
                                         : BarcodeFormat.QR_CODE
                        };

                        if (BarcodeHelper.ValidateBarcode(card.CardNumber, card.DisplayType, out string errorMessage))
                        {
                            importedCards.Add(card);
                        }
                        else
                        {
                            invalidCards.Add(card);
                            Console.WriteLine($"Invalid barcode for card '{card.CardName}': {errorMessage}");
                        }
                    }
                }
                return importedCards;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Import Error: {ex.Message}");
                return new ObservableCollection<Card>();
            }
        }

        public string ExportCardsToText(ObservableCollection<Card> cards)
        {
            var optimizedCards = OptimizeCardData(cards);
            string serializedCards = SerializeCardDictionaries(optimizedCards);
            return CompressJson(serializedCards);
        }

        private List<Dictionary<string, object>> OptimizeCardData(ObservableCollection<Card> cards)
        {
            var optimizedCards = new List<Dictionary<string, object>>();

            foreach (var card in cards)
            {
                var optimizedCard = new Dictionary<string, object>
                {
                    ["n"] = card.CardName,
                    ["k"] = card.CardNickname,
                    ["c"] = card.CardNumber,
                    ["t"] = (int)card.DisplayType
                };
                optimizedCards.Add(optimizedCard);
            }

            return optimizedCards;
        }

        private List<Dictionary<string, object>> DeserializeCardDictionaries(string json)
        {
            var dictionaries = new List<Dictionary<string, object>>();
            if (string.IsNullOrEmpty(json)) return dictionaries;

            json = json.Trim('[', ']');

            if (string.IsNullOrEmpty(json)) return dictionaries;

            foreach (var cardJson in json.Split(new[] { "}," }, StringSplitOptions.None))
            {
                var dict = new Dictionary<string, object>();
                var parts = cardJson.Trim('{', '}').Split(',');
                foreach (var part in parts)
                {
                    var keyValue = part.Split(':');
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim('\"');
                        var value = keyValue[1].Trim('\"');

                        if (!string.IsNullOrEmpty(value) && !value.Equals("null", StringComparison.OrdinalIgnoreCase))
                        {
                            dict[key] = value;
                        }
                    }
                }
                dictionaries.Add(dict);
            }

            return dictionaries;
        }

        private string SerializeCardDictionaries(List<Dictionary<string, object>> dictionaries)
        {
            var sb = new StringBuilder("[");
            foreach (var dict in dictionaries)
            {
                sb.Append("{");
                foreach (var kvp in dict)
                {
                    sb.Append($"\"{kvp.Key}\":\"{kvp.Value}\",");
                }

                if (dict.Count > 0) sb.Length--;
                sb.Append("},");
            }
            if (dictionaries.Count > 0) sb.Length--;
            sb.Append("]");

            return sb.ToString();
        }

        public static string CompressJson(string json)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                using (var writer = new StreamWriter(gzipStream, Encoding.UTF8))
                {
                    writer.Write(json);
                }
                byte[] compressedBytes = memoryStream.ToArray();
                return Convert.ToBase64String(compressedBytes);
            }
        }

        public static string DecompressJson(string compressedData)
        {
            byte[] compressedBytes = Convert.FromBase64String(compressedData);

            using (var memoryStream = new MemoryStream(compressedBytes))
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzipStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}