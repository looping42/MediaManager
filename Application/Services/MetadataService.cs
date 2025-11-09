using Domain.Movie.Dto;
using MediaManager.Domain.Interface;

namespace MediaManager.Application.Services
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