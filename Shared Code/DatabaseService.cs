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
            return Path.Combine(FileSystem.AppDataDirectory, "cards.db");
        }
    }
}
