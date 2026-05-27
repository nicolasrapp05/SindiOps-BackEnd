namespace SindiOps.API.Infrastructure.Storage;

public interface IStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType);
    Task<string> GetSignedUrlAsync(string filePath, int expiresInSeconds = 3600);
    Task DeleteAsync(string filePath);
}
