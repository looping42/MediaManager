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
            var existingNfos = new HashSet<string>(_db.GetAllMovies().Select(m => m.NfoPath));

            int addedCount = 0;
            int skippedCount = 0;
            var moviesToInsert = new ConcurrentBag<Movie>();
            Parallel.ForEach(Directory.EnumerateFiles(directoryPath, "*.nfo", SearchOption.AllDirectories), nfoFile =>
            {
                try
                {
                    if (existingNfos.Contains(nfoFile))
                    {
                        skippedCount++;
                        return;
                    }

                    var xml = XDocument.Load(nfoFile);
                    var title = xml.Root?.Element("title")?.Value ?? Path.GetFileNameWithoutExtension(nfoFile);
                    var year = xml.Root?.Element("year")?.Value ?? "";
                    var plot = xml.Root?.Element("plot")?.Value ?? "";

                    var imdbId = xml.Root?.Element("id")?.Value
                        ?? xml.Root?.Element("imdbid")?.Value
                        ?? "";

                    var folder = Path.GetDirectoryName(nfoFile);

                    // Enumerate all files in the folder once
                    var folderFiles = Directory.EnumerateFiles(folder).ToList();

                    // Find the files in memory instead of hitting disk multiple times
                    var poster = folderFiles.FirstOrDefault(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && f.Contains("poster", StringComparison.OrdinalIgnoreCase))
                                 ?? folderFiles.FirstOrDefault(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase));

                    string posterThumb = null;
                    if (poster != null)
                    {
                        posterThumb = Path.Combine(folder, "thumb_" + Path.GetFileName(poster));
                        if (!File.Exists(posterThumb))
                        {
                            // Generate a small thumbnail
                            using var image = System.Drawing.Image.FromFile(poster);
                            int thumbWidth = 50;
                            int thumbHeight = (int)((double)image.Height / image.Width * thumbWidth);
                            using var thumb = new Bitmap(image, new System.Drawing.Size(thumbWidth, thumbHeight));
                            thumb.Save(posterThumb, ImageFormat.Jpeg);
                        }
                    }

                    var fanart = folderFiles.FirstOrDefault(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && f.Contains("fanart", StringComparison.OrdinalIgnoreCase));

                    var clearLogo = folderFiles.FirstOrDefault(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && f.Contains("clearlogo", StringComparison.OrdinalIgnoreCase));

                    moviesToInsert.Add(new Movie
                    {
                        Title = title,
                        Year = year,
                        Plot = plot,
                        NfoPath = nfoFile,
                        PosterUrl = poster,
                        PosterThumb = posterThumb,  // small image for ListBox
                        FanartUrl = fanart,
                        ClearLogoUrl = clearLogo,
                        ImdbId = imdbId
                    });

                    addedCount++;
                }
                catch { /* Ignore fichiers corrompus */ }
            });
            _db.InsertMovie(moviesToInsert);

            MessageBox.Show(
           $"Scan terminé ✅\n" +
           $"{addedCount} nouveau(x) film(s) ajouté(s)\n" +
           $"{skippedCount} déjà existant(s)",
           "Scan terminé",
           MessageBoxButton.OK,
           MessageBoxImage.Information);
        }
    }
}