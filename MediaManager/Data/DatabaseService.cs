using Dapper;
using MediaManager.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Data
{
    public class DatabaseService
    {
        private readonly string _dbPath;

        public DatabaseService(string dbPath = "movies.db")
        {
            _dbPath = dbPath;
            if (!File.Exists(_dbPath))
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();
                connection.Execute(@"
            CREATE TABLE Movies (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT,
                Year TEXT,
                Plot TEXT,
                PosterUrl TEXT,
                PosterThumb TEXT,
                FanartUrl TEXT,
                ClearLogoUrl TEXT,
                ImdbId TEXT,
                NfoPath TEXT
            );
        ");
            }
        }

        public bool MovieExistsByNfoPath(string nfoPath)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            return connection.QueryFirstOrDefault<int>(
                "SELECT COUNT(1) FROM Movies WHERE NfoPath = @Path",
                new { Path = nfoPath }
            ) > 0;
        }

        public void InsertMovie(ConcurrentBag<Movie> moviesToInsert)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            using var transaction = connection.BeginTransaction();

            foreach (var movie in moviesToInsert)
            {
                connection.Execute(@"
                    INSERT INTO Movies
                    (Title, Year, Plot, PosterUrl, PosterThumb, FanartUrl, ClearLogoUrl, ImdbId, NfoPath)
                    VALUES (@Title, @Year, @Plot, @PosterUrl, @PosterThumb, @FanartUrl, @ClearLogoUrl, @ImdbId, @NfoPath)",
                    movie, transaction: transaction);
            }

            transaction.Commit();
        }

        public Movie GetMovieById(int id)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            return connection.QueryFirstOrDefault<Movie>(
                "SELECT * FROM Movies WHERE Id = @Id",
                new { Id = id }
            );
        }

        public IEnumerable<Movie> GetAllMovies(string search = null)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            if (string.IsNullOrWhiteSpace(search))
            {
                // Retourne tous les films triés par titre
                return connection.Query<Movie>("SELECT * FROM Movies ORDER BY Title COLLATE NOCASE");
            }
            else
            {
                // Retourne les films filtrés par titre et triés
                return connection.Query<Movie>(
                    "SELECT * FROM Movies WHERE Title LIKE @Search ORDER BY Title COLLATE NOCASE",
                    new { Search = $"%{search}%" });
            }
        }
    }
}