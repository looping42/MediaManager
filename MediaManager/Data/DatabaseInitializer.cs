using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(string dbPath = "movies.db")
        {
            DatabaseConnectionFactory.SetDatabasePath(dbPath);

            if (!File.Exists(dbPath))
            {
                using var connection = DatabaseConnectionFactory.CreateConnection();
                connection.Open();

                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS Movies (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT,
                        Year TEXT,
                        FolderUrl TEXT,
                        NfoUrl TEXT UNIQUE,
                        FilmUrl TEXT,
                        PosterUrl TEXT,
                        FanartUrl TEXT,
                        ClearLogoUrl TEXT,
                        ThumbsUrl TEXT,
                        LastWriteUtc TEXT
                    );

                    CREATE TABLE IF NOT EXISTS Settings (
                        Key TEXT PRIMARY KEY,
                        Value TEXT
                    );
                ");
            }
        }
    }
}