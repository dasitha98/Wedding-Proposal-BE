using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;

namespace Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;

public class Lifestyle
{
    public LifestyleHabit Smoking { get; private set; }
    public LifestyleHabit Alcohol { get; private set; }

    private Lifestyle()
    {
    }

    public static Lifestyle CreateDefault() => new()
    {
        Smoking = LifestyleHabit.PreferNotToSay,
        Alcohol = LifestyleHabit.PreferNotToSay
    };

    public static Lifestyle Create(LifestyleHabit smoking, LifestyleHabit alcohol) =>
        new() { Smoking = smoking, Alcohol = alcohol };
}
