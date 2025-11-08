using MediaManager.Data;
using MediaManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MediaManager.Business.ApiSearch
{
    public class TmdbMetadataProvider : IMovieMetadataProvider
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly SettingsRepository _settingsRepo;

        public TmdbMetadataProvider()
        {
            _settingsRepo = new SettingsRepository();
        }

        public async Task<MovieMetadata> FetchMetadataAsync(string title, string year = null)
        {
            if (string.IsNullOrEmpty(title))
                return null;

            var apikey = _settingsRepo.Get(ConstantSettings.ApiKeyKey) ?? "";
            var paths = _settingsRepo.GetPaths("ScanPaths");
            string query = Uri.EscapeDataString(title);
            string url = $"https://api.themoviedb.org/3/search/movie?api_key={apikey}&query={query}";
            if (!string.IsNullOrEmpty(year))
                url += $"&year={year}";

            var response = await _httpClient.GetFromJsonAsync<JsonDocument>(url);
            if (response.RootElement.GetProperty("results").GetArrayLength() == 0)
                return null;

            var movie = response.RootElement.GetProperty("results")[0];

            string posterPath = movie.GetProperty("poster_path").GetString();
            string backdropPath = movie.GetProperty("backdrop_path").GetString();

            return new MovieMetadata
            {
                Title = movie.GetProperty("title").GetString(),
                Year = movie.GetProperty("release_date").GetString()?.Split('-')[0],
                Plot = movie.GetProperty("overview").GetString(),
                PosterUrl = !string.IsNullOrEmpty(posterPath) ? $"https://image.tmdb.org/t/p/original{posterPath}" : null,
                FanartUrl = !string.IsNullOrEmpty(backdropPath) ? $"https://image.tmdb.org/t/p/original{backdropPath}" : null
            };
        }
    }
}