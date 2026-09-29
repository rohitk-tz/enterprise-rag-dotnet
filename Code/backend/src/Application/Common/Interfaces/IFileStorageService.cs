namespace Application.Common.Interfaces;

public interface IFileStorageService
{
    string GeneratePresignedUploadUrl(string s3Key, string contentType);
    Task DeleteAsync(string s3Key, CancellationToken ct);
    Task<Stream> DownloadAsync(string s3Key, CancellationToken ct);
}
