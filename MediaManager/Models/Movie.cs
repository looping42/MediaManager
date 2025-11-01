using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Models
{
    public class Movie
    {
        public int Id { get; set; }             // Id INTEGER PRIMARY KEY AUTOINCREMENT

        public string Title { get; set; }       // Title TEXT
        public string Year { get; set; }        // Year TEXT

        public string FolderUrl { get; set; }   // FolderUrl TEXT
        public string NfoUrl { get; set; }      // NfoUrl TEXT
        public string FilmUrl { get; set; }     // FilmUrl TEXT

        public string PosterUrl { get; set; }   // PosterUrl TEXT
        public string FanartUrl { get; set; }   // FanartUrl TEXT
        public string ClearLogoUrl { get; set; }// ClearLogoUrl TEXT
        public string ThumbsUrl { get; set; }   // ThumbsUrl TEXT

        public string LastWriteUtc { get; set; }// LastWriteUtc TEXT
    }
}
