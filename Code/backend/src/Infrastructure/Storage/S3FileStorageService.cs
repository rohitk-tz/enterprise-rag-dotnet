using Amazon.S3;
using Amazon.S3.Model;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Storage;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3FileStorageService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["S3:BucketName"]
            ?? throw new InvalidOperationException("S3:BucketName is not configured.");
    }

    public string GeneratePresignedUploadUrl(string s3Key, string contentType)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = s3Key,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.AddHours(1)
        };

        return _s3Client.GetPreSignedURL(request);
    }

    public Task DeleteAsync(string s3Key, CancellationToken ct) =>
        _s3Client.DeleteObjectAsync(_bucketName, s3Key, ct);

    public async Task<Stream> DownloadAsync(string s3Key, CancellationToken ct)
    {
        var response = await _s3Client.GetObjectAsync(_bucketName, s3Key, ct);
        return response.ResponseStream;
    }
}
