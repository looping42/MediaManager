using Dapper;
using MediaManager.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Data
{
    public class MovieRepository
    {
        public bool MovieExistsByNfoPath(string nfoPath)
        {
            using var connection = DatabaseConnectionFactory.CreateConnection();
            connection.Open();

            return connection.QueryFirstOrDefault<int>(
                "SELECT COUNT(1) FROM Movies WHERE NfoUrl = @Path",
                new { Path = nfoPath }
            ) > 0;
        }

        public void InsertMovies(IEnumerable<Movie> movies)
        {
            if (movies == null || !movies.Any())
                return;

            using var connection = DatabaseConnectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            const string sql = @"
                INSERT INTO Movies
                (Title, Year, FolderUrl, NfoUrl, FilmUrl, PosterUrl, FanartUrl, ClearLogoUrl, ThumbsUrl, LastWriteUtc)
                VALUES (@Title, @Year, @FolderUrl, @NfoUrl, @FilmUrl, @PosterUrl, @FanartUrl, @ClearLogoUrl, @ThumbsUrl, @LastWriteUtc);
            ";

            connection.Execute(sql, movies, transaction: transaction);
            transaction.Commit();
        }

        public void UpdateMovie(Movie movie)
        {
            if (movie == null) return;

            using var connection = DatabaseConnectionFactory.CreateConnection();
            connection.Open();

            const string sql = @"
        INSERT INTO Movies
        (Title, Year, FolderUrl, NfoUrl, FilmUrl, PosterUrl, FanartUrl, ClearLogoUrl, ThumbsUrl, LastWriteUtc)
        VALUES
        (@Title, @Year, @FolderUrl, @NfoUrl, @FilmUrl, @PosterUrl, @FanartUrl, @ClearLogoUrl, @ThumbsUrl, @LastWriteUtc)
        ON CONFLICT(NfoUrl) DO UPDATE SET
            Title = excluded.Title,
            Year = excluded.Year,
            FolderUrl = excluded.FolderUrl,
            FilmUrl = excluded.FilmUrl,
            PosterUrl = excluded.PosterUrl,
            FanartUrl = excluded.FanartUrl,
            ClearLogoUrl = excluded.ClearLogoUrl,
            ThumbsUrl = excluded.ThumbsUrl,
            LastWriteUtc = excluded.LastWriteUtc;
    ";

            connection.Execute(sql, movie);
        }

        public Movie GetMovieById(int id)
        {
            using var connection = DatabaseConnectionFactory.CreateConnection();
            connection.Open();

            return connection.QueryFirstOrDefault<Movie>(
                "SELECT * FROM Movies WHERE Id = @Id",
                new { Id = id }
            );
        }

        public IEnumerable<Movie> GetAllMovies(string? search = null)
        {
            using var connection = DatabaseConnectionFactory.CreateConnection();
            connection.Open();

            if (string.IsNullOrWhiteSpace(search))
                return connection.Query<Movie>("SELECT * FROM Movies ORDER BY Title COLLATE NOCASE");

            return connection.Query<Movie>(
                "SELECT * FROM Movies WHERE Title LIKE @Search ORDER BY Title COLLATE NOCASE",
                new { Search = $"%{search}%" });
        }
    }
}