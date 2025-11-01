using Dapper;
using MediaManager.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Service
{
    public class MovieMetadataFetcher
    {
        private readonly string _apiKey;
        private readonly HttpClient _http = new();

        public MovieMetadataFetcher(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<TmdbMovie?> FetchFromTitleAsync(string title, string? year = null)
        {
            var url = $"https://api.themoviedb.org/3/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(title)}";
            if (!string.IsNullOrEmpty(year))
                url += $"&year={year}";

            var response = await _http.GetFromJsonAsync<TmdbSearchResult>(url);
            var movie = response?.Results?.FirstOrDefault();
            if (movie == null) return null;

            // Récupère les détails (IMDB ID etc.)
            var detailsUrl = $"https://api.themoviedb.org/3/movie/{movie.Id}?api_key={_apiKey}&append_to_response=images";
            return await _http.GetFromJsonAsync<TmdbMovie>(detailsUrl);
        }
    }
}