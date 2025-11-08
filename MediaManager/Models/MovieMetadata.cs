using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Models
{
    public class MovieMetadata
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string Plot { get; set; }
        public string ImdbId { get; set; }
        public string PosterUrl { get; set; }
        public string FanartUrl { get; set; }
        public string ClearLogoUrl { get; set; }
        public string BannerUrl { get; set; }
    }
}