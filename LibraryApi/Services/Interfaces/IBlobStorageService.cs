using Microsoft.AspNetCore.Http;

namespace LibraryApi.Services.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadBookImageAsync(IFormFile file);

    Task DeleteAsync(string imageUrl);
}