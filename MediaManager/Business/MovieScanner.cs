using MediaManager.Data;
using MediaManager.Logger;
using MediaManager.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;

namespace MediaManager.Business
{
    public class MovieScanner
    {
        private readonly MovieRepository _movieRepo;

        public event Action? ScanCompleted;

        public event Action<Movie>? MovieScanned;

        private readonly IUiLogger _logger;
        private readonly SettingsRepository _settingsRepo;

        public MovieScanner(MovieRepository movieRepo, SettingsRepository settingsRepo, IUiLogger logger)
        {
            _movieRepo = movieRepo;
            _settingsRepo = settingsRepo;
            _logger = logger;
        }

        public void ScanDirectoryNfoOnly(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                _logger.Log($"Folder not found: {directoryPath}");
            }
            var ignoredPath = _settingsRepo.GetPaths(ConstantSettings.IgnoredPath);

            var allMovies = _movieRepo.GetAllMovies();
            var existingNfos = new HashSet<string>(allMovies.Select(m => m.NfoUrl));
            var moviesToInsert = new List<Movie>();

            var nfoFiles = Directory.EnumerateFiles(directoryPath, "*.nfo", SearchOption.AllDirectories)
                .Where(f => !existingNfos.Contains(f) && !ignoredPath.Contains(f));

            var moviesByNfo = allMovies.ToDictionary(m => m.NfoUrl, m => m);
            var validNfoFiles = new List<string>();
            foreach (var nfoFile in nfoFiles)
            {
                var folder = Path.GetDirectoryName(nfoFile);
                DateTime lastWrite = Directory.GetLastWriteTimeUtc(folder);

                // On vérifie s’il existe déjà dans la DB
                if (moviesByNfo.TryGetValue(nfoFile, out var existingMovie))
                {
                    // Si le dossier n’a pas changé depuis, on saute
                    if (DateTime.TryParse(existingMovie.LastWriteUtc, out var lastWriteDb)
                        && lastWrite <= lastWriteDb)
                    {
                        continue;
                    }
                }

                if (!File.Exists(nfoFile))
                {
                    _logger.Log($"NFO not found ignored : {nfoFile}");
                    continue;
                }

                string title = "";
                using (var reader = XmlReader.Create(nfoFile))
                {
                    title = ReadElementSafe(reader, "title");
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    _logger.Log($"NFO invalid ignored: {Path.GetFileName(nfoFile)}");
                    continue;
                }

                moviesToInsert.Add(new Movie
                {
                    Title = title,
                    Year = "",
                    FolderUrl = folder,
                    NfoUrl = nfoFile,
                    FilmUrl = "",
                    PosterUrl = "",
                    FanartUrl = "",
                    ClearLogoUrl = "",
                    ThumbsUrl = "",
                    LastWriteUtc = lastWrite.ToString("o"),
                    IsIdentified = true
                });
                validNfoFiles.Add(nfoFile);
            }

            _movieRepo.InsertMovies(moviesToInsert);

            //Lance le scan complet en tâche de fond
            Task.Run(() =>
            {
                ScanUnidentifiedMovies(directoryPath);

                foreach (var nfoFile in validNfoFiles)
                {
                    ScanDirectoryMetadata(nfoFile); // récupère images et infos détaillées
                }
            });
        }

        /// <summary>
        /// Scan complet : lit les NFO et récupère posters, fanart, etc.
        /// </summary>
        private void ScanDirectoryMetadata(string nfoFile)
        {
            try
            {
                var folder = Path.GetDirectoryName(nfoFile);
                DateTime lastWrite = Directory.GetLastWriteTimeUtc(folder);

                string title = "";
                string year = "";

                using (var reader = XmlReader.Create(nfoFile))
                {
                    title = ReadElementSafe(reader, "title");
                    year = ReadElementSafe(reader, "year");
                }

                if (string.IsNullOrEmpty(title)) return;

                var files = Directory.GetFiles(folder);

                var movie = new Movie
                {
                    Title = title,
                    Year = year,
                    FolderUrl = folder,
                    NfoUrl = nfoFile,
                    FilmUrl = files.FirstOrDefault(f => f.EndsWith(".mkv") || f.EndsWith(".mp4")) ?? "",
                    PosterUrl = files.FirstOrDefault(f => f.ToLower().Contains("poster")) ?? "",
                    FanartUrl = files.FirstOrDefault(f => f.ToLower().Contains("fanart")) ?? "",
                    ClearLogoUrl = files.FirstOrDefault(f => f.ToLower().Contains("clearlogo")) ?? "",
                    ThumbsUrl = files.FirstOrDefault(f => f.ToLower().Contains("thumb")) ?? "",
                    LastWriteUtc = lastWrite.ToString("o"),
                };

                _movieRepo.UpdateMovie(movie); // méthode qui update ou insert
            }
            catch (Exception ex)
            {
                _logger.Log($"Error: {ex.ToString()}");
                // ignorer les erreurs
            }
        }

        public void ScanUnidentifiedMovies(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) return;

            var allMovies = _movieRepo.GetAllMovies();
            var existingFolders = new HashSet<string>(allMovies.Select(m => m.FolderUrl));
            var existingTitles = new HashSet<string>(allMovies.Select(m => m.Title), StringComparer.OrdinalIgnoreCase);

            var moviesToInsert = new List<Movie>();
            var ignoredPath = _settingsRepo.GetPaths(ConstantSettings.IgnoredPath);

            var files = Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories)
                    .Where(f => !ignoredPath.Contains(f));

            foreach (var folder in files)
            {
                // Ignorer si déjà dans la DB
                if (existingFolders.Contains(folder)) continue;

                var videoFile = Directory.GetFiles(folder)
                    .FirstOrDefault(f => f.EndsWith(".mkv") || f.EndsWith(".mp4"));

                if (videoFile != null)
                {
                    var title = Path.GetFileName(folder);

                    // Vérifier si un film avec ce titre existe déjà
                    if (existingTitles.Contains(title)) continue;

                    var movie = new Movie
                    {
                        Title = title,
                        Year = "",
                        FolderUrl = folder,
                        NfoUrl = null,  // Aucun NFO
                        FilmUrl = videoFile,
                        PosterUrl = "",
                        FanartUrl = "",
                        ClearLogoUrl = "",
                        ThumbsUrl = "",
                        LastWriteUtc = Directory.GetLastWriteTimeUtc(folder).ToString("o"),
                        IsIdentified = false
                    };

                    moviesToInsert.Add(movie);
                    _logger.Log($"Movie non identifié ajouté : {movie.Title}");
                }
                _movieRepo.InsertMovies(moviesToInsert);
            }
        }

        private string ReadElementSafe(XmlReader reader, string elementName)
        {
            try
            {
                if (reader.ReadToFollowing(elementName))
                {
                    return reader.ReadElementContentAsString() ?? "";
                }
            }
            catch (Exception ex)
            {
                //_log?.Invoke($"Error reader: {ex.ToString()}");
            }
            return "";
        }
    }
}