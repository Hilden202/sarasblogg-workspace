using System.Text;
using System.Text.Json;
using SarasBlogg.DTOs;
using SarasBlogg.Models;

namespace SarasBlogg.DAL
{
    public class BloggAPIManager
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };

        public BloggAPIManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Blogg>> GetAllBloggsAsync()
        {
            var resp = await _httpClient.GetAsync("api/Blogg");
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GET all bloggs failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {body}");
            }

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Blogg>>(json, _jsonOpts) ?? new List<Blogg>();
        }

        public async Task<Blogg?> GetBloggAsync(int id)
        {
            var resp = await _httpClient.GetAsync($"api/Blogg/{id}");
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"GET blogg {id} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {body}");
            }

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Blogg>(json, _jsonOpts);
        }

        public async Task<Blogg?> SaveBloggAsync(Blogg blogg)
        {
            var content = new StringContent(JsonSerializer.Serialize(blogg, _jsonOpts), Encoding.UTF8, "application/json");
            var resp = await _httpClient.PostAsync("api/Blogg", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"POST blogg failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {body}");
            }

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Blogg>(json, _jsonOpts);
        }

        public async Task UpdateBloggAsync(Blogg blogg)
        {
            var content = new StringContent(JsonSerializer.Serialize(blogg, _jsonOpts), Encoding.UTF8, "application/json");
            var resp = await _httpClient.PutAsync($"api/Blogg/{blogg.Id}", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"PUT blogg {blogg.Id} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {body}");
            }
        }

        public async Task<bool> ToggleHiddenAsync(int id)
        {
            var resp = await _httpClient.PatchAsync($"api/Blogg/{id}/hidden", null);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"PATCH hidden failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {body}");
            }
            return true;
        }

        public async Task<bool> ToggleArchivedAsync(int id)
        {
            var resp = await _httpClient.PatchAsync($"api/Blogg/{id}/archived", null);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"PATCH archived failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {body}");
            }
            return true;
        }


        public async Task DeleteBloggAsync(int id)
        {
            var resp = await _httpClient.DeleteAsync($"api/Blogg/{id}");

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"DELETE blogg {id} failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {body}");
            }
        }

        public async Task<string?> GetEditorAccessTokenAsync()
        {
            var resp = await _httpClient.GetAsync("api/auth/editor-token");

            if (!resp.IsSuccessStatusCode)
                return null;

            var contentType = resp.Content.Headers.ContentType?.MediaType;

            // 🔒 Säkerhetsbälte: API måste svara JSON
            if (!string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                var json = await resp.Content.ReadAsStringAsync();
                var dto = JsonSerializer.Deserialize<AccessTokenDto>(json, _jsonOpts);
                return dto?.AccessToken;
            }
            catch (JsonException)
            {
                // 🔇 Tyst fail – admin-sidan ska inte dö
                return null;
            }
        }
    }
}
