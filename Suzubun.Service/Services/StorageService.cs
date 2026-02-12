using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Suzubun.Service.Models;

namespace Suzubun.Service.Services;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder);
    Task DeleteFileAsync(string fileUrl);
}

public class StorageService : IStorageService
{
    private readonly CloudflareR2Options _options;
    private readonly IAmazonS3 _s3Client;

    public StorageService(IOptions<AppOptions> options)
    {
        _options = options.Value.CloudflareR2;
        
        var config = new AmazonS3Config
        {
            ServiceURL = _options.ServiceUrl,
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(_options.AccessKeyId, _options.SecretAccessKey, config);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder)
    {
        var key = $"{folder.Trim('/')}/{Guid.NewGuid()}_{fileName}";
        
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = fileStream,
            DisablePayloadSigning = true // R2 requirement for some regions
        };

        await _s3Client.PutObjectAsync(request);

        return $"{_options.PublicUrl}/{key}";
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        var uri = new Uri(fileUrl);
        var key = uri.AbsolutePath.TrimStart('/');

        await _s3Client.DeleteObjectAsync(_options.BucketName, key);
    }
}
