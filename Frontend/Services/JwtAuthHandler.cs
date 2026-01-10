using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SarasBlogg.Services
{
    public sealed class JwtAuthHandler : DelegatingHandler
    {
        private readonly IAccessTokenStore _store;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<JwtAuthHandler> _logger;

        public JwtAuthHandler(
            IAccessTokenStore store,
            IHttpContextAccessor http,
            ILogger<JwtAuthHandler> logger)
        {
            _store = store;
            _http = http;
            _logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken ct)
        {
            // ❌ TinyMCE / editor-upload ska aldrig få Bearer-token
            if (request.RequestUri?.AbsolutePath.StartsWith(
                    "/api/editor",
                    StringComparison.OrdinalIgnoreCase) == true)
            {
                return base.SendAsync(request, ct);
            }

            // ✅ Lägg Authorization om den saknas
            if (request.Headers.Authorization is null)
            {
                // 1️⃣ Försök från minnet (satt vid login/callback)
                var token = _store.AccessToken;

                // 2️⃣ Fallback: HttpOnly-cookie (överlever reloads)
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = _http.HttpContext?.Request?.Cookies["api_access_token"];
                }

                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    _logger.LogDebug(
                        "JwtAuthHandler: Bearer token attached for {Method} {Url}",
                        request.Method,
                        request.RequestUri);
                }
                else
                {
                    _logger.LogDebug(
                        "JwtAuthHandler: No token available, request sent anonymous: {Method} {Url}",
                        request.Method,
                        request.RequestUri);
                }
            }

            return base.SendAsync(request, ct);
        }
    }
}
