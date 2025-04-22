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
        public ObservableCollection<Card> ImportCardsFromTextAsync(string compressedBase64Export, out List<Card> invalidCards)
        {
            invalidCards = new List<Card>();
            var importedCards = new ObservableCollection<Card>();
            long nextId = 1;
            List<Dictionary<string, object>> importedDictionaries = null;

            try
            {
                string json = DecompressJson(compressedBase64Export);
                if (string.IsNullOrEmpty(json)) return importedCards;

                importedDictionaries = DeserializeCardMetadataManual(json);
            }
            catch
            {
                return importedCards;
            }

            if (importedDictionaries != null)
            {
                foreach (var dict in importedDictionaries)
                {
                    Card card = null;
                    try
                    {
                        string cardName = dict.ContainsKey("n") ? Convert.ToString(dict["n"]) : string.Empty;
                        string cardNickname = dict.ContainsKey("k") ? Convert.ToString(dict["k"]) : string.Empty;
                        string encodedCardNumber = dict.ContainsKey("c") ? Convert.ToString(dict["c"]) : string.Empty;
                        BarcodeFormat displayType = dict.ContainsKey("t") && dict["t"] != null && Enum.TryParse(dict["t"].ToString(), out BarcodeFormat format)
                                                 ? format
                                                 : BarcodeFormat.QR_CODE;

                        string cardNumber = DecodeCardNumber(encodedCardNumber);

                        if (string.IsNullOrEmpty(encodedCardNumber) && !string.IsNullOrEmpty(cardNumber))
                        {
                            cardNumber = string.Empty;
                        }

                        card = new Card
                        {
                            Id = nextId,
                            CardName = cardName,
                            CardNickname = cardNickname,
                            CardNumber = cardNumber,
                            DisplayType = displayType
                        };

                        if (BarcodeHelper.ValidateBarcode(card.CardNumber, card.DisplayType, out string errorMessage))
                        {
                            card.Id = nextId++;
                            importedCards.Add(card);
                        }
                        else
                        {
                            invalidCards.Add(card);
                        }
                    }
                    catch
                    {
                        if (card == null)
                        {
                            card = new Card
                            {
                                Id = nextId,
                                CardName = dict.ContainsKey("n") ? Convert.ToString(dict["n"]) : "Error Card",
                                CardNickname = dict.ContainsKey("k") ? Convert.ToString(dict["k"]) : string.Empty,
                                CardNumber = "Error during processing",
                                DisplayType = BarcodeFormat.QR_CODE
                            };
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(card.CardNumber)) card.CardNumber = "Error during processing";
                        }
                        invalidCards.Add(card);
                    }
                }
            }

            return importedCards;
        }

        public string ExportCardsToText(ObservableCollection<Card> cards)
        {
            try
            {
                string json = SerializeCardMetadataManual(cards);
                return CompressJson(json);
            }
            catch
            {
                return string.Empty;
            }
        }

        private string SerializeCardMetadataManual(ObservableCollection<Card> cards)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            bool firstCard = true;

            foreach (var card in cards)
            {
                if (!firstCard) sb.Append(",");
                firstCard = false;

                sb.Append("{");

                sb.Append("\"n\":\"");
                sb.Append(JsonEscape(card.CardName ?? string.Empty));
                sb.Append("\",");

                sb.Append("\"k\":\"");
                sb.Append(JsonEscape(card.CardNickname ?? string.Empty));
                sb.Append("\",");

                string encodedC = EncodeCardNumber(card.CardNumber);
                sb.Append("\"c\":\"");
                sb.Append(encodedC);
                sb.Append("\",");

                sb.Append("\"t\":");
                sb.Append(((int)card.DisplayType).ToString());

                sb.Append("}");
            }

            sb.Append("]");
            return sb.ToString();
        }

        private List<Dictionary<string, object>> DeserializeCardMetadataManual(string json)
        {
            var dictionaries = new List<Dictionary<string, object>>();
            if (string.IsNullOrWhiteSpace(json)) return dictionaries;

            json = json.Trim();
            if (!json.StartsWith("[") || !json.EndsWith("]") || json.Length <= 2) return dictionaries;

            string arrayContent = json.Substring(1, json.Length - 2).Trim();
            if (string.IsNullOrEmpty(arrayContent)) return dictionaries;

            int currentIndex = 0;
            int length = arrayContent.Length;

            while (currentIndex < length)
            {
                while (currentIndex < length && (char.IsWhiteSpace(arrayContent[currentIndex]) || arrayContent[currentIndex] == ',')) { currentIndex++; }
                if (currentIndex >= length) break;

                if (arrayContent[currentIndex] != '{') break;

                int objectEndIndex = FindMatchingBrace(arrayContent, currentIndex);
                if (objectEndIndex == -1) break;

                string singleObjectJson = arrayContent.Substring(currentIndex, objectEndIndex - currentIndex + 1);

                try
                {
                    var dict = ParseSingleObjectManualSimplified(singleObjectJson);
                    if (dict != null)
                    {
                        dictionaries.Add(dict);
                    }
                }
                catch
                {
                    dictionaries.Add(new Dictionary<string, object> { { "parseError", true } });
                }

                currentIndex = objectEndIndex + 1;
            }

            return dictionaries;
        }

        private Dictionary<string, object> ParseSingleObjectManualSimplified(string objectJson)
        {
            var dict = new Dictionary<string, object>();
            if (string.IsNullOrWhiteSpace(objectJson) || objectJson.Length < 2 || !objectJson.StartsWith("{") || !objectJson.EndsWith("}")) return dict;
            string content = objectJson.Substring(1, objectJson.Length - 2).Trim();
            if (string.IsNullOrEmpty(content)) return dict;

            int index = 0;
            int len = content.Length;

            while (index < len)
            {
                int keyStartQuote = content.IndexOf('"', index);
                if (keyStartQuote == -1) break;
                int keyEndQuote = content.IndexOf('"', keyStartQuote + 1);
                if (keyEndQuote == -1) break;
                string key = content.Substring(keyStartQuote + 1, keyEndQuote - keyStartQuote - 1);

                int colonIndex = content.IndexOf(':', keyEndQuote + 1);
                if (colonIndex == -1) break;

                int valueStartIndex = colonIndex + 1;
                while (valueStartIndex < len && char.IsWhiteSpace(content[valueStartIndex])) { valueStartIndex++; }
                if (valueStartIndex >= len) break;

                object value = null;
                int valueEndIndex = valueStartIndex;
                char firstValueChar = content[valueStartIndex];

                if (firstValueChar == '"')
                {
                    valueEndIndex = FindEndOfJsonString(content, valueStartIndex + 1);
                    if (valueEndIndex == -1) break;
                    string rawValue = content.Substring(valueStartIndex + 1, valueEndIndex - valueStartIndex - 1);
                    value = (key == "c") ? rawValue : UnescapeJsonString(rawValue);
                    valueEndIndex++;
                }
                else
                {
                    int nextComma = content.IndexOf(',', valueStartIndex);
                    valueEndIndex = (nextComma != -1) ? nextComma : len;
                    string rawValue = content.Substring(valueStartIndex, valueEndIndex - valueStartIndex).Trim();
                    if (key == "t" && int.TryParse(rawValue, out int intVal)) { value = intVal; }
                    else if (rawValue.Equals("null", StringComparison.OrdinalIgnoreCase)) { value = null; }
                    else if (rawValue.Equals("true", StringComparison.OrdinalIgnoreCase)) { value = true; }
                    else if (rawValue.Equals("false", StringComparison.OrdinalIgnoreCase)) { value = false; }
                    else { value = rawValue; }
                }

                if (value != null) { dict[key] = value; }

                int nextCommaIndex = content.IndexOf(',', valueEndIndex);
                if (nextCommaIndex != -1) { index = nextCommaIndex + 1; }
                else { break; }
            }
            return dict;
        }

        private string EncodeCardNumber(string rawCardNumber)
        {
            if (string.IsNullOrEmpty(rawCardNumber)) return string.Empty;
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(rawCardNumber);
                return Convert.ToBase64String(data);
            }
            catch
            {
                return string.Empty;
            }
        }

        private string DecodeCardNumber(string base64EncodedCardNumber)
        {
            if (string.IsNullOrEmpty(base64EncodedCardNumber)) return string.Empty;
            try
            {
                byte[] data = Convert.FromBase64String(base64EncodedCardNumber);
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return string.Empty;
            }
        }

        private string JsonEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: if (c < ' ') { continue; } sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        private string UnescapeJsonString(string escaped)
        {
            if (string.IsNullOrEmpty(escaped) || escaped.IndexOf('\\') == -1) return escaped;
            var sb = new StringBuilder(escaped.Length); bool escaping = false;
            for (int i = 0; i < escaped.Length; i++)
            {
                char c = escaped[i];
                if (escaping)
                {
                    switch (c)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (i + 4 < escaped.Length) { string hex = escaped.Substring(i + 1, 4); if (ushort.TryParse(hex, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out ushort uval)) { sb.Append(Convert.ToChar(uval)); i += 4; } else { sb.Append("\\u"); } } else { sb.Append("\\u"); }
                            break;
                        default: sb.Append(c); break;
                    }
                    escaping = false;
                }
                else if (c == '\\') { escaping = true; } else { sb.Append(c); }
            }
            return sb.ToString();
        }

        private int FindEndOfJsonString(string text, int startIndex)
        {
            int length = text.Length;
            for (int i = startIndex; i < length; i++)
            {
                if (text[i] == '"') { int backslashCount = 0; int j = i - 1; while (j >= 0 && text[j] == '\\') { backslashCount++; j--; } if (backslashCount % 2 == 1) { continue; } else { return i; } }
            }
            return -1;
        }

        private int FindMatchingBrace(string text, int startIndex)
        {
            if (startIndex < 0 || startIndex >= text.Length || text[startIndex] != '{') return -1;
            int braceLevel = 0; bool inString = false;
            for (int i = startIndex; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '"') { int backslashCount = 0; int j = i - 1; while (j >= 0 && text[j] == '\\') { backslashCount++; j--; } if (backslashCount % 2 == 0) { inString = !inString; } }
                if (!inString) { if (c == '{') braceLevel++; else if (c == '}') { braceLevel--; if (braceLevel == 0) return i; if (braceLevel < 0) return -1; } }
            }
            return -1;
        }

        public static string CompressJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return string.Empty;
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                using (var memoryStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                    {
                        gzipStream.Write(buffer, 0, buffer.Length);
                    }
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string DecompressJson(string compressedBase64Data)
        {
            if (string.IsNullOrEmpty(compressedBase64Data)) return string.Empty;
            try
            {
                byte[] buffer = Convert.FromBase64String(compressedBase64Data);
                using (var memoryStream = new MemoryStream(buffer))
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                using (var reader = new StreamReader(gzipStream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch
            {
                throw;
            }
        }

    }
}