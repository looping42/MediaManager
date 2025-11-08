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
    public class FanartMetadataProvider : IMovieMetadataProvider
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly SettingsRepository _settingsRepo;

        public FanartMetadataProvider()
        {
            _settingsRepo = new SettingsRepository();
        }

        public async Task<MovieMetadata> FetchMetadataAsync(string title, string year = null)
        {
            if (string.IsNullOrEmpty(title))
                return null;

            var apikey = _settingsRepo.Get(ConstantSettings.ApiKeyKey) ?? "";
            string url = $"https://webservice.fanart.tv/v3/movies/{Uri.EscapeDataString(title)}?api_key={apikey}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<JsonDocument>(url);

                var root = response.RootElement;

                string posterUrl = null;
                if (root.TryGetProperty("movieposter", out var posterArray) && posterArray.ValueKind == JsonValueKind.Array && posterArray.GetArrayLength() > 0)
                {
                    var firstPoster = posterArray[0];
                    if (firstPoster.TryGetProperty("url", out var urlProp))
                        posterUrl = urlProp.GetString();
                }

                string fanartUrl = null;
                if (root.TryGetProperty("moviebackground", out var fanartArray) && fanartArray.ValueKind == JsonValueKind.Array && fanartArray.GetArrayLength() > 0)
                {
                    var firstFanart = fanartArray[0];
                    if (firstFanart.TryGetProperty("url", out var urlProp))
                        fanartUrl = urlProp.GetString();
                }

                string clearLogoUrl = null;
                if (root.TryGetProperty("movielogo", out var logoArray) && logoArray.ValueKind == JsonValueKind.Array && logoArray.GetArrayLength() > 0)
                {
                    var firstLogo = logoArray[0];
                    if (firstLogo.TryGetProperty("url", out var urlProp))
                        clearLogoUrl = urlProp.GetString();
                }

                return new MovieMetadata
                {
                    PosterUrl = posterUrl,
                    FanartUrl = fanartUrl,
                    ClearLogoUrl = clearLogoUrl
                };
            }
            catch
            {
                return null;
            }
        }
    }
}