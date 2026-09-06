namespace Wedding_Proposal_BE.Shared.Infrastructure.Storage;

public class DigitalOceanSpacesOptions
{
    public const string SectionName = "DigitalOceanSpaces";

    // Origin endpoint the S3-compatible SDK talks to for uploads, e.g. "https://sgp1.digitaloceanspaces.com".
    public string ServiceUrl { get; set; } = "";

    public string BucketName { get; set; } = "";

    // CDN endpoint used to build the public URL returned to clients, e.g.
    // "https://mangala.sgp1.cdn.digitaloceanspaces.com".
    public string CdnBaseUrl { get; set; } = "";

    public string AccessKey { get; set; } = "";

    public string SecretKey { get; set; } = "";
}
