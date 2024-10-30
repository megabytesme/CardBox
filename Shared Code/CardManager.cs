using System.Runtime.Serialization.Json;

namespace Shared_Code
{
    public class CardManager
    {
        public static async Task<List<Card>> LoadCardAsync()
        {
            var roamingFolder = ApplicationData.Current.RoamingFolder;
            StorageFile cardsListFile = await roamingFolder.TryGetItemAsync("cardList.json") as StorageFile;

            if (cardsListFile == null)
            {
                cardsListFile = await roamingFolder.CreateFileAsync("cardList.json", CreationCollisionOption.ReplaceExisting);
                DataContractJsonSerializer writeSerializer = new DataContractJsonSerializer(typeof(List<Card>));
                using (Stream stream = await cardsListFile.OpenStreamForWriteAsync())
                {
                    writeSerializer.WriteObject(stream, new List<Card>());
                }
            }

            DataContractJsonSerializer readSerializer = new DataContractJsonSerializer(typeof(List<Card>));
            using (Stream stream = await cardsListFile.OpenStreamForReadAsync())
            {
                var files = (List<Card>)readSerializer.ReadObject(stream);
                return files ?? new List<Card>();
            }
        }

        public static async Task SaveCardAsync(List<Card> card)
        {
            var roamingFolder = ApplicationData.Current.RoamingFolder;
            StorageFile cardsListFile = await roamingFolder.GetFileAsync("cardList.json");
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Card>));
            using (Stream stream = await cardsListFile.OpenStreamForWriteAsync())
            {
                serializer.WriteObject(stream, cardsListFile);
            }
        }
    }
}
