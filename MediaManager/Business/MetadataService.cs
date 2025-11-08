using MediaManager.Business.ApiSearch;
using MediaManager.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaManager.Business
{
    public class MovieMetadataService
    {
        private readonly List<IMovieMetadataProvider> _providers;

        public MovieMetadataService(IEnumerable<IMovieMetadataProvider> providers)
        {
            _providers = providers.ToList();
        }

        public async Task<MovieMetadata> FetchMetadataAsync(string title, string year = null)
        {
            var finalMetadata = new MovieMetadata();

            // Boucle sur chaque provider
            foreach (var provider in _providers)
            {
                var data = await provider.FetchMetadataAsync(title, year);
            }

            return finalMetadata;
        }
    }
}