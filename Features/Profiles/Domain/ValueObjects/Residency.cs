namespace Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;

public class Residency
{
    public string City { get; private set; } = string.Empty;
    public string District { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;

    private Residency()
    {
    }

    public static Residency CreateDefault() => new();

    public static Residency Create(string city, string district, string country) =>
        new() { City = city, District = district, Country = country };
}
