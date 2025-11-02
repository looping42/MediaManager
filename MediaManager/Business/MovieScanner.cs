using MediaManager.Data;
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

        private readonly Action<string> _log;

        public MovieScanner(MovieRepository movieRepo, Action<string> log)
        {
            _movieRepo = movieRepo;
            _log = log;
        }

        public void ScanDirectoryNfoOnly(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                _log?.Invoke($"Folder not found: {directoryPath}");
            }

            var allMovies = _movieRepo.GetAllMovies();
            var existingNfos = new HashSet<string>(allMovies.Select(m => m.NfoUrl));
            var moviesToInsert = new List<Movie>();

            var nfoFiles = Directory.EnumerateFiles(directoryPath, "*.nfo", SearchOption.AllDirectories)
                .Where(f => !existingNfos.Contains(f) &&
                            !f.Contains(".deletedByTMM") &&
                            !f.Contains(".HomeTheater"));

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
                    _log?.Invoke($"NFO not found ignored : {nfoFile}");
                    return;
                }

                string title = "";
                using (var reader = XmlReader.Create(nfoFile))
                {
                    title = ReadElementSafe(reader, "title");
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    _log?.Invoke($"NFO invalid ignored: {Path.GetFileName(nfoFile)}");
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
                });
                validNfoFiles.Add(nfoFile);
            }
            //var duplicates = moviesToInsert
            //    .GroupBy(m => m.FolderUrl)
            //    .Where(g => g.Count() > 1)
            //    .Select(g => new { Folder = g.Key, Count = g.Count(), Titles = g.Select(x => x.Title) })
            //    .ToList();

            //moviesToInsert = moviesToInsert
            //    .GroupBy(m => m.FolderUrl)
            //    .Select(g => g.First()) // garde un seul élément par dossier
            //    .ToList();

            _movieRepo.InsertMovies(moviesToInsert);

            //Lance le scan complet en tâche de fond
            Task.Run(() =>
            {
                foreach (var nfoFile in validNfoFiles)
                {
                    ScanDirectoryMetadata(nfoFile); // récupère images et infos détaillées
                }

                //// 🔹 Étape 2 : recherche des films sans NFO en tâche de fond
                //foreach (var folder in directoryPath)
                //{
                //    try
                //    {
                //        _movieScanner.ScanUnidentifiedMovies(folder); // nouvelle méthode dédiée
                //    }
                //    catch (Exception ex)
                //    {
                //        Log($"Erreur détection films sans NFO dans {folder}: {ex.Message}");
                //    }
                //}
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
                _log?.Invoke($"Error: {ex.ToString()}");
                // ignorer les erreurs
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