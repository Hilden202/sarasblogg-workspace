namespace SarasBloggAPI.Services.AboutMe
{
    public interface IAboutMeImageService
    {
        Task<string?> GetCurrentUrlAsync();
        Task<string?> UploadOrReplaceAsync(IFormFile file);
        Task DeleteAsync();
    }
}
