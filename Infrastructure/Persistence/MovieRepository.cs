using Dapper;
using MediaManager.Domain;
using MediaManager.Domain.Interface;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Infrastructure.Persistence
{
    public class MovieRepository : IMovieRepository
    {
        public async Task<bool> MovieExistsByNfoPathAsync(string nfoPath)
        {
            using var connection = DatabaseConnectionFactory.CreateConnection();
            connection.Open();

            return connection.QueryFirstOrDefault<int>(
                "SELECT COUNT(1) FROM Movies WHERE NfoUrl = @Path",
                new { Path = nfoPath }
            ) > 0;
        }

        public async Task InsertMoviesAsync(IEnumerable<Movie> movies)
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

        public async Task UpdateMovieAsync(Movie movie)
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

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            using var connection = DatabaseConnectionFactory.CreateConnection();
            connection.Open();

            return connection.QueryFirstOrDefault<Movie>(
                "SELECT * FROM Movies WHERE Id = @Id",
                new { Id = id }
            );
        }

        public async Task<IEnumerable<Movie>> GetAllMoviesAsync(string? search = null)
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