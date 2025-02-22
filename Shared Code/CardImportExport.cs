using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Shared_Code
{
    public class CardImportExport
    {
        public ObservableCollection<Card> ImportCardsFromTextAsync(string importedText)
        {
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
                        var card = new Card();
                        card.Id = nextId++;
                        if (dict.ContainsKey("n")) card.CardName = (string)dict["n"];
                        if (dict.ContainsKey("k")) card.CardNickname = (string)dict["k"];
                        if (dict.ContainsKey("c")) card.CardNumber = (string)dict["c"];
                        if (dict.ContainsKey("t"))
                        {
                            if (long.TryParse(dict["t"].ToString(), out long tValue))
                            {
                                try
                                {
                                    card.DisplayType = (DisplayType)(int)tValue;
                                }
                                catch (OverflowException ex)
                                {
                                    Console.WriteLine($"Overflow Exception: {ex.Message}");
                                    card.DisplayType = DisplayType.QrCode;
                                }
                            }
                        }
                        importedCards.Add(card);
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
                var optimizedCard = new Dictionary<string, object>();
                optimizedCard["n"] = card.CardName;
                optimizedCard["k"] = card.CardNickname;
                optimizedCard["c"] = card.CardNumber;
                optimizedCard["t"] = (int)card.DisplayType;
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