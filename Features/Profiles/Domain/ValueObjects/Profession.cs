using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;

public class Profession
{
    public JobStatus JobStatus { get; private set; }
    public string Occupation { get; private set; } = string.Empty;
    public IncomeRange IncomeRange { get; private set; }

    private Profession()
    {
    }

    public static Profession CreateDefault() => new()
    {
        JobStatus = JobStatus.Other,
        Occupation = string.Empty,
        IncomeRange = IncomeRange.PreferNotToSay
    };

    public static Profession Create(JobStatus jobStatus, string occupation, IncomeRange incomeRange) =>
        new() { JobStatus = jobStatus, Occupation = occupation, IncomeRange = incomeRange };
}
