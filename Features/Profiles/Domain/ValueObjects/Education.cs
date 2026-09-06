using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;

public class Education
{
    public EducationStatus Qualification { get; private set; }
    public QualificationStatus QualificationStatus { get; private set; }

    private Education()
    {
    }

    public static Education CreateDefault() => new()
    {
        Qualification = EducationStatus.Other,
        QualificationStatus = QualificationStatus.Completed
    };

    public static Education Create(EducationStatus qualification, QualificationStatus status) =>
        new() { Qualification = qualification, QualificationStatus = status };
}
