using SQLite;
using System.IO;

namespace Shared_Code
{
    public class DatabaseService
    {
        private static readonly string DbPath = GetDatabasePath();
        private SQLiteConnection _database;

        public DatabaseService()
        {
            _database = new SQLiteConnection(DbPath);
            _database.CreateTable<Card>();
        }

        public SQLiteConnection Connection => _database;

        private static string GetDatabasePath()
        {

#if WINDOWS_UWP
            return Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "cards.db");
#else
            return Path.Combine(FileSystem.AppDataDirectory, "cards.db");
#endif
        }
    }
}
