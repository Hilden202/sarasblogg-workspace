namespace SarasBlogg.Services
{
    public interface IAccessTokenStore
    {
        string? AccessToken { get; }
        void Set(string token);
        void Clear();
    }

    public sealed class CookieAccessTokenStore : IAccessTokenStore
    {
        private readonly IHttpContextAccessor _http;

        public CookieAccessTokenStore(IHttpContextAccessor http)
        {
            _http = http;
        }

        public string? AccessToken
            => _http.HttpContext?.Request?.Cookies["api_access_token"];

        public void Set(string token)
        {
            // gör inget – cookien sätts redan vid login
        }

        public void Clear()
        {
            _http.HttpContext?.Response?.Cookies.Delete("api_access_token");
        }
    }

}
