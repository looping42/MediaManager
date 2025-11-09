using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Movie.Dto
{
    public class TmdbSearchResult
    {
        public List<TmdbMovie> Results { get; set; } = new();
    }
}