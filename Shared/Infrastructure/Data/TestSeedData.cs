using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Auth.Domain.Entities;
using Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;
using Wedding_Proposal_BE.Features.Messages.Domain.Entities;
using Wedding_Proposal_BE.Features.Profiles.Domain.Entities;
using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;
using Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Shared.Infrastructure.Data;

/// Opt-in test data for a real database: inserts 5 rows into every data table (Users,
/// UserRoles, Profiles, ProfileFavourites, ProfileConnectionRequests, Conversations,
/// Messages, RefreshTokens, EmailVerificationOtps, PasswordResetOtps). Roles is left alone —
/// it only ever holds the 3 core roles. Runs when "SeedTestData" is true and is idempotent:
/// it does nothing if the first test user already exists. All test accounts use
/// <see cref="Password"/> and emails ending in @test.wedproposal.app.
public static class TestSeedData
{
    public const string Password = "Test@12345";
    public const string OtpCode = "123456";

    private static readonly (string First, string Last, Gender Gender, string Religion, string Race, string City, string District, string Occupation)[] People =
    [
        ("Amaya", "Fernando", Gender.Female, "Buddhist", "Sinhalese", "Colombo", "Colombo", "Software Engineer"),
        ("Kavindu", "Perera", Gender.Male, "Buddhist", "Sinhalese", "Kandy", "Kandy", "Doctor"),
        ("Priya", "Sivakumar", Gender.Female, "Hindu", "Tamil", "Jaffna", "Jaffna", "Teacher"),
        ("Mohamed", "Rizwan", Gender.Male, "Islam", "Muslim", "Negombo", "Gampaha", "Accountant"),
        ("Ruwani", "de Silva", Gender.Female, "Christian", "Burgher", "Galle", "Galle", "Architect"),
    ];

    private static readonly string[] Messages =
    [
        "Hi! I saw your profile and thought we might get along.",
        "Hello, thanks for accepting my request!",
        "Nice to meet you. What are you looking for in a partner?",
        "Are you free for a call sometime this week?",
        "Looking forward to getting to know you better.",
    ];

    public static string EmailFor(int index) =>
        $"{People[index].First.ToLowerInvariant()}.{People[index].Last.ToLowerInvariant().Replace(" ", "")}@test.wedproposal.app";

    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<AppDbContext>();

        if (await userManager.FindByEmailAsync(EmailFor(0)) is not null)
        {
            logger.LogInformation("Test seed data already present — skipping.");
            return;
        }

        // Users + UserRoles (5 each).
        var users = new List<ApplicationUser>();
        for (var i = 0; i < People.Length; i++)
        {
            var person = People[i];
            var user = ApplicationUser.Create(EmailFor(i), person.First, person.Last);
            user.EmailConfirmed = true;

            var created = await userManager.CreateAsync(user, Password);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Creating test user {user.Email} failed: {string.Join(", ", created.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(user, RoleNames.User);
            users.Add(user);
        }

        // Profiles (5).
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var profiles = new List<Profile>();
        for (var i = 0; i < People.Length; i++)
        {
            var person = People[i];
            var profile = Profile.CreateBlank(users[i].Id, person.First, person.Last, today.AddYears(-(25 + i)));

            var isCasteEligible = person.Religion is "Buddhist" or "Hindu";
            profile.UpdateBasicInfo(BasicInfo.Create(
                person.Gender, person.Race, person.Religion,
                isCasteEligible ? "Other" : string.Empty,
                MaritalStatus.NeverMarried,
                $"5' {4 + i}\""));
            profile.UpdateEducation(Education.Create(
                (EducationStatus)(i % Enum.GetValues<EducationStatus>().Length),
                QualificationStatus.Completed));
            profile.UpdateProfession(Profession.Create(
                JobStatus.Employed,
                person.Occupation,
                (IncomeRange)(i % (Enum.GetValues<IncomeRange>().Length - 1))));
            profile.UpdateResidency(Residency.Create(person.City, person.District, "Sri Lanka"));
            profile.UpdateLifestyle(Lifestyle.Create(LifestyleHabit.No, LifestyleHabit.No));
            profile.UpdateAssets(Assets.Create(AssetsStatus.PreferNotToSay));
            profile.UpdateVerification(Verification.Create(
                phoneVerified: true, emailVerified: true, identityVerified: i % 2 == 0,
                photoVerified: false, professionVerified: false, educationVerified: false));
            profile.ReplacePhotos([($"https://picsum.photos/seed/wedtest{i}/600/600", false)]);
            profile.UpdateBasics(
                person.First, person.Last, ManagedBy.Self,
                $"Hi, I'm {person.First}! This is a test profile.",
                ["Reading", "Traveling", "Music"],
                ["Family-oriented"]);

            dbContext.Profiles.Add(profile);
            profiles.Add(profile);
        }

        // Each user i is paired with user (i + 1) % 5 — five distinct pairs, so no
        // favourite / request / conversation duplicates another.
        for (var i = 0; i < users.Count; i++)
        {
            var me = users[i];
            var next = users[(i + 1) % users.Count];

            // ProfileFavourites (5).
            dbContext.ProfileFavourites.Add(ProfileFavourite.Create(me.Id, profiles[(i + 1) % profiles.Count].Id));

            // ProfileConnectionRequests (5): mix of pending / accepted / declined.
            var request = ProfileConnectionRequest.Create(me.Id, next.Id);
            if (i is 1 or 3) request.Accept();
            if (i is 4) request.Decline();
            dbContext.ProfileConnectionRequests.Add(request);

            // Conversations (5) + Messages (5, one per conversation).
            var conversation = Conversation.Create(me.Id, next.Id);
            dbContext.Conversations.Add(conversation);
            dbContext.Messages.Add(Message.Create(conversation.Id, me.Id, Messages[i]));

            // RefreshTokens (5).
            dbContext.RefreshTokens.Add(RefreshToken.Issue(me.Id, $"test-refresh-token-{i}-{Guid.NewGuid():N}", TimeSpan.FromDays(7)));

            // EmailVerificationOtps / PasswordResetOtps (5 each). Code is OtpCode for all.
            dbContext.EmailVerificationOtps.Add(EmailVerificationOtp.Issue(
                me.Email!, OtpCodeGenerator.Hash(OtpCode), TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(60)));
            dbContext.PasswordResetOtps.Add(PasswordResetOtp.Issue(
                me.Email!, OtpCodeGenerator.Hash(OtpCode), TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(60)));
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Inserted test seed data: 5 rows in each data table.");
    }
}
