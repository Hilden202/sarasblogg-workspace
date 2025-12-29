using Microsoft.AspNetCore.Http;
using SarasBloggAPI.Services;

namespace SarasBloggAPITests.TestHelpers;

public sealed class FakeFileHelper : IFileHelper
{
    public Task<string?> SaveImageAsync(IFormFile file, string folderName)
        => Task.FromResult<string?>("https://fake.local/image.jpg");

    public Task<string?> SaveImageAsync(IFormFile file, int bloggId, string folderName = "blogg")
        => Task.FromResult<string?>($"https://fake.local/{bloggId}/image.jpg");

    public Task DeleteImageAsync(string imageUrl, string folder)
        => Task.CompletedTask;

    public Task DeleteBlogFolderAsync(int bloggId, string folderName = "blogg")
        => Task.CompletedTask;
}
