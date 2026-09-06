using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;

public class Assets
{
    public AssetsStatus Status { get; private set; }

    private Assets()
    {
    }

    public static Assets CreateDefault() => new() { Status = AssetsStatus.PreferNotToSay };

    public static Assets Create(AssetsStatus status) => new() { Status = status };
}
