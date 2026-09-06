using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace Wedding_Proposal_BE.Shared.Infrastructure.Storage;

public class DigitalOceanSpacesStorageService : IObjectStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly DigitalOceanSpacesOptions _options;

    public DigitalOceanSpacesStorageService(IAmazonS3 s3Client, IOptions<DigitalOceanSpacesOptions> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead,
            AutoCloseStream = false,
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        return $"{_options.CdnBaseUrl.TrimEnd('/')}/{key}";
    }
}
