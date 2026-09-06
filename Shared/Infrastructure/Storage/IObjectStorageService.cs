namespace Wedding_Proposal_BE.Shared.Infrastructure.Storage;

public interface IObjectStorageService
{
    // Uploads content as a public-read object and returns its public (CDN) URL.
    Task<string> UploadAsync(Stream content, string key, string contentType, CancellationToken cancellationToken = default);
}
