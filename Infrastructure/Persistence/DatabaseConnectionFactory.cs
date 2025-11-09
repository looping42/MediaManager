using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Infrastructure.Persistence
{
    public static class DatabaseConnectionFactory
    {
        private static string _dbPath = "movies.db";

        public static void SetDatabasePath(string path)
        {
            _dbPath = path;
        }

        public static SqliteConnection CreateConnection()
        {
            return new SqliteConnection($"Data Source={_dbPath}");
        }
    }
}