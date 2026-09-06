using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Options;

namespace Wedding_Proposal_BE.Shared.Infrastructure.Storage;

public static class StorageServiceRegistration
{
    public static IServiceCollection AddObjectStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DigitalOceanSpacesOptions>(configuration.GetSection(DigitalOceanSpacesOptions.SectionName));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<DigitalOceanSpacesOptions>>().Value;
            var config = new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = false,
                // DigitalOcean Spaces doesn't support the SDK v4 default of always attaching
                // request/response checksums, which otherwise fails every request against it.
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
            };

            return new AmazonS3Client(new BasicAWSCredentials(options.AccessKey, options.SecretKey), config);
        });

        services.AddSingleton<IObjectStorageService, DigitalOceanSpacesStorageService>();

        return services;
    }
}
