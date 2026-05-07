using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SarasBlogg.DTOs;

namespace SarasBlogg.DAL
{
    public class CommentAPIManager
    {
        private readonly HttpClient _httpClient;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public CommentAPIManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- Nya DTO-baserade anrop (med TopRole) ---

        public async Task<List<DTOs.CommentWithRoleDto>> GetByBloggWithRolesAsync(int bloggId)
        {
            var resp = await _httpClient.GetAsync($"api/Comment/by-blogg/{bloggId}");
            if (!resp.IsSuccessStatusCode) return new List<DTOs.CommentWithRoleDto>();

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DTOs.CommentWithRoleDto>>(json, _jsonOpts)
                   ?? new List<DTOs.CommentWithRoleDto>();
        }

        public async Task<List<DTOs.CommentWithRoleDto>> GetAllCommentsWithRolesAsync()
        {
            var resp = await _httpClient.GetAsync("api/Comment");
            if (!resp.IsSuccessStatusCode) return new List<DTOs.CommentWithRoleDto>();

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DTOs.CommentWithRoleDto>>(json, _jsonOpts)
                   ?? new List<DTOs.CommentWithRoleDto>();
        }

        public async Task<DTOs.CommentWithRoleDto?> GetCommentWithRoleAsync(int id)
        {
            var resp = await _httpClient.GetAsync($"api/Comment/ById/{id}");
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DTOs.CommentWithRoleDto>(json, _jsonOpts);
        }

        // --- Bakåtkompatibel metod (mappar DTO -> gamla modellen) ---

        public async Task<Models.Comment?> GetCommentAsync(int id)
        {
            var d = await GetCommentWithRoleAsync(id);
            if (d == null) return null;

            return new Models.Comment
            {
                Id = d.Id,
                BloggId = d.BloggId,
                Name = d.Name,
                Email = null,
                Content = d.Content ?? "",
                CreatedAt = d.CreatedAt
            };
        }

        // --- Skapa / radera ---

        public async Task<string?> SaveCommentAsync(Models.Comment comment)
        {
            var request = new CommentCreateRequest
            {
                BloggId = comment.BloggId,
                Name = string.IsNullOrWhiteSpace(comment.Name) ? null : comment.Name.Trim(),
                Content = comment.Content ?? string.Empty
            };

            var content = new StringContent(JsonSerializer.Serialize(request, _jsonOpts), Encoding.UTF8, "application/json");
            var resp = await _httpClient.PostAsync("api/Comment", content);

            if (resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadAsStringAsync();
        }

        public async Task DeleteCommentAsync(int id)
        {
            await _httpClient.DeleteAsync($"api/Comment/ById/{id}");
        }

        public async Task DeleteCommentsAsync(int bloggId)
        {
            await _httpClient.DeleteAsync($"api/Comment/ByBlogg/{bloggId}");
        }
    }
}
