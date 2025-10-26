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
        private readonly DatabaseService _db;

        public MovieScanner(DatabaseService db)
        {
            _db = db;
        }

        public void ScanDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                MessageBox.Show("Le dossier spécifié n'existe pas.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //var existingNfos = new HashSet<string>(_db.GetAllMovies().Select(m => m.NfoPath));
            var existingNfos = new HashSet<string>(_db.GetAllMovies().Select(m => m.NfoPath));

            int addedCount = 0;
            int skippedCount = 0;
            var moviesToInsert = new List<Movie>();

            //var folders = Directory.EnumerateDirectories(directoryPath).ToList();

            var nfoFiles = Directory.EnumerateFiles(directoryPath, "*.nfo", SearchOption.AllDirectories)
                        .Where(f => !existingNfos.Contains(f));

            foreach (var nfoFile in nfoFiles)
            {
                //var nfoFile = Directory.EnumerateFiles(folder, "*.nfo", SearchOption.TopDirectoryOnly).FirstOrDefault();
                //if (nfoFile == null)
                //{
                //    skippedCount++;
                //    continue;
                //}

                //if (existingNfos.Contains(nfoFile))
                //{
                //    skippedCount++;
                //    continue;
                //}

                string title = Path.GetFileNameWithoutExtension(nfoFile);
                string year = "";
                string plot = "";
                string imdbId = "";

                using (var reader = XmlReader.Create(nfoFile))
                {
                    year = ReadElementSafe(reader, "year");
                    plot = ReadElementSafe(reader, "plot");

                    // Pour id ou imdbid, lire d'abord id puis imdbid si id est vide
                    imdbId = ReadElementSafe(reader, "id");
                    if (string.IsNullOrEmpty(imdbId))
                        imdbId = ReadElementSafe(reader, "imdbid");
                }

                //var xml = XDocument.Load(nfoFile);
                //var title = xml.Root?.Element("title")?.Value ?? Path.GetFileNameWithoutExtension(nfoFile);
                //var year = xml.Root?.Element("year")?.Value ?? "";
                //var plot = xml.Root?.Element("plot")?.Value ?? "";

                //var imdbId = xml.Root?.Element("id")?.Value
                //    ?? xml.Root?.Element("imdbid")?.Value
                //    ?? "";

                var folder = Path.GetDirectoryName(nfoFile);

                //// Enumerate all files in the folder once
                //var folderFiles = Directory.EnumerateFiles(folder).ToList();

                //// Find the files in memory instead of hitting disk multiple times
                //var poster = folder.FirstOrDefault(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && f.Contains("poster", StringComparison.OrdinalIgnoreCase))
                //             ?? folder.FirstOrDefault(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase));

                //var fanart = folder.FirstOrDefault(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && f.Contains("fanart", StringComparison.OrdinalIgnoreCase));

                //var clearLogo = folder.FirstOrDefault(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && f.Contains("clearlogo", StringComparison.OrdinalIgnoreCase));

                var poster = "";
                var fanart = "";
                var clearLogo = "";

                moviesToInsert.Add(new Movie
                {
                    Title = title,
                    Year = year,
                    Plot = plot,
                    NfoPath = nfoFile,
                    PosterUrl = poster,
                    PosterThumb = "",  // small image for ListBox
                    FanartUrl = fanart,
                    ClearLogoUrl = clearLogo,
                    ImdbId = imdbId
                });
                addedCount++;
            }

            _db.InsertMovie(moviesToInsert);

            MessageBox.Show(
           $"Scan terminé ✅\n" +
           $"{addedCount} nouveau(x) film(s) ajouté(s)\n" +
           $"{skippedCount} déjà existant(s)",
           "Scan terminé",
           MessageBoxButton.OK,
           MessageBoxImage.Information);
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
            catch
            {
                // ignore erreurs de lecture
            }
            return "";
        }
    }
}