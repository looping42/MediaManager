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
    public class DatabaseInitializer
    {
        private readonly string _dbPath;

        public DatabaseInitializer(string dbPath = "movies.db")
        {
            _dbPath = dbPath;
        }

        public void Initialize()
        {
            DatabaseConnectionFactory.SetDatabasePath(_dbPath);

            bool dbExists = File.Exists(_dbPath);

            if (!File.Exists(_dbPath))
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
                        LastWriteUtc TEXT,
                        IsIdentified INTEGER DEFAULT 0
                    );

                    CREATE TABLE IF NOT EXISTS Settings (
                        Key TEXT PRIMARY KEY,
                        Value TEXT
                    );
                ");
            }

            // Si la DB vient d'être créée, insérer les chemins à ignorer par défaut
            if (!dbExists)
            {
                var defaultIgnoredPaths = new List<string> { ".deletedByTMM", ".HomeTheater" };
                foreach (var path in defaultIgnoredPaths)
                {
                    using var connection = DatabaseConnectionFactory.CreateConnection();
                    connection.Open();
                    connection.Execute(@"
                        INSERT OR IGNORE INTO Settings (Key, Value) VALUES (@Key, @Value);
                    ", new { Key = "IgnoredPath:" + path, Value = path });
                }
            }
        }
    }
}