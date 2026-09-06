using Microsoft.AspNetCore.Identity;
using Wedding_Proposal_BE.Features.Messages.Domain.Entities;
using Wedding_Proposal_BE.Features.Profiles.Domain.Entities;
using Wedding_Proposal_BE.Features.Profiles.Domain.Enums;
using Wedding_Proposal_BE.Features.Profiles.Domain.ValueObjects;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Shared.Infrastructure.Data;

/// Dev-only convenience: seeds one login-able demo account plus ~30 browsable profiles into
/// the in-memory database, so Discover/Messages/Profile screens have real data to show
/// without registering a dozen accounts by hand. Only ever called against the in-memory
/// database (see Program.cs) — never against a real Postgres database. Photos use a public
/// placeholder-image service (picsum.photos) purely for dev visuals, not real uploads.
public static class DevSeedData
{
    public const string DemoEmail = "demo@wedproposal.app";
    public const string DemoPassword = "Password123";

    private static readonly (string First, string Last)[] Names =
    [
        ("Amaya", "Fernando"), ("Kavindu", "Perera"), ("Nethmi", "Silva"), ("Tharindu", "Jayasuriya"),
        ("Ishara", "Wickramasinghe"), ("Dilan", "Gunasekara"), ("Sanduni", "Rathnayake"), ("Chamod", "Bandara"),
        ("Vindya", "Weerasinghe"), ("Nuwan", "Karunaratne"), ("Priya", "Sivakumar"), ("Arun", "Rajendran"),
        ("Kavya", "Thiruchelvam"), ("Mohamed", "Rizwan"), ("Fathima", "Nazeer"), ("Ahamed", "Sameer"),
        ("Ruwani", "de Silva"), ("Shehan", "Abeysekara"), ("Dinithi", "Mendis"), ("Yohan", "Fonseka"),
        ("Sachini", "Dissanayake"), ("Roshan", "Peiris"), ("Anjali", "Kularatne"), ("Malith", "Senanayake"),
        ("Hansika", "Ranasinghe"), ("Chathura", "Amarasinghe"), ("Nadeesha", "Ekanayake"), ("Lahiru", "Herath"),
        ("Kumari", "Wijesinghe"), ("Janith", "Gamage"),
    ];

    private static readonly string[] Religions = ["Buddhist", "Hindu", "Islam", "Christian", "Catholic"];
    private static readonly string[] Races = ["Sinhalese", "Tamil", "Muslim", "Moor", "Burgher"];
    private static readonly string[] Castes = ["Govigama", "Karava", "Vellalar", "Karaiyar", "Other"];
    private static readonly MaritalStatus[] MaritalStatuses =
        [MaritalStatus.NeverMarried, MaritalStatus.Divorced, MaritalStatus.Widowed, MaritalStatus.NeverMarried, MaritalStatus.NeverMarried];
    private static readonly string[] Occupations =
        ["Software Engineer", "Doctor", "Teacher", "Accountant", "Business Owner", "Registered Nurse", "Civil Engineer", "Lawyer", "Architect", "Marketing Manager"];
    private static readonly string[] ForeignCountries = ["United Kingdom", "Australia", "Canada", "United States", "Germany"];
    private static readonly (string District, string[] Cities)[] SriLankaAreas =
    [
        ("Colombo", ["Colombo", "Dehiwala", "Nugegoda"]),
        ("Gampaha", ["Negombo", "Ja-Ela", "Wattala"]),
        ("Kandy", ["Kandy", "Peradeniya"]),
        ("Galle", ["Galle", "Hikkaduwa"]),
        ("Jaffna", ["Jaffna", "Chavakachcheri"]),
    ];
    private static readonly string[] HobbyPool =
        ["Reading", "Traveling", "Cooking", "Cricket", "Music", "Photography", "Hiking", "Yoga", "Dancing", "Gardening"];
    private static readonly string[] InterestPool =
        ["Marriage-minded", "Open to relocation", "Family-oriented", "Traditional values", "Modern outlook"];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<AppDbContext>();

        if (await userManager.FindByEmailAsync(DemoEmail) is null)
        {
            var newDemoUser = ApplicationUser.Create(DemoEmail, "Demo", "User");
            newDemoUser.EmailConfirmed = true;
            await userManager.CreateAsync(newDemoUser, DemoPassword);
            await userManager.AddToRoleAsync(newDemoUser, RoleNames.User);

            var demoDateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-27));
            dbContext.Profiles.Add(Profile.CreateBlank(newDemoUser.Id, newDemoUser.FirstName, newDemoUser.LastName, demoDateOfBirth));
            await dbContext.SaveChangesAsync();
        }

        if (dbContext.Profiles.Count() > 1)
        {
            // Already seeded (e.g. a prior run against a persistent in-memory instance).
            return;
        }

        var demoUser = await userManager.FindByEmailAsync(DemoEmail)
            ?? throw new InvalidOperationException("Demo user should have been created above.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var seeded = new List<(ApplicationUser User, Profile Profile)>();

        for (var i = 0; i < Names.Length; i++)
        {
            var (firstName, lastName) = Names[i];
            var email = $"{firstName.ToLowerInvariant().Replace(" ", "")}.{lastName.ToLowerInvariant().Replace(" ", "")}{i}@wedproposal.app";

            var user = ApplicationUser.Create(email, firstName, lastName);
            user.EmailConfirmed = i % 3 == 0;
            await userManager.CreateAsync(user);
            await userManager.AddToRoleAsync(user, RoleNames.User);

            var gender = i % 2 == 0 ? Gender.Male : Gender.Female;
            var religion = Religions[i % Religions.Length];
            var isCasteEligible = religion is "Buddhist" or "Hindu";
            var isForeign = i % 6 == 0;
            var age = 20 + (i % 25);
            var dateOfBirth = today.AddYears(-age);

            var profile = Profile.CreateBlank(user.Id, firstName, lastName, dateOfBirth);

            profile.UpdateBasicInfo(BasicInfo.Create(
                gender,
                Races[i % Races.Length],
                religion,
                isCasteEligible ? Castes[i % Castes.Length] : string.Empty,
                MaritalStatuses[i % MaritalStatuses.Length],
                $"{5 + i % 2}' {i % 12}\""));

            profile.UpdateEducation(Education.Create(
                (EducationStatus)(i % Enum.GetValues<EducationStatus>().Length),
                QualificationStatus.Completed));

            profile.UpdateProfession(Profession.Create(
                JobStatus.Employed,
                Occupations[i % Occupations.Length],
                (IncomeRange)(i % (Enum.GetValues<IncomeRange>().Length - 1))));

            if (isForeign)
            {
                profile.UpdateResidency(Residency.Create(string.Empty, string.Empty, ForeignCountries[i % ForeignCountries.Length]));
            }
            else
            {
                var (district, cities) = SriLankaAreas[i % SriLankaAreas.Length];
                profile.UpdateResidency(Residency.Create(cities[i % cities.Length], district, "Sri Lanka"));
            }

            profile.UpdateLifestyle(Lifestyle.Create(LifestyleHabit.No, i % 4 == 0 ? LifestyleHabit.Occasionally : LifestyleHabit.No));
            profile.UpdateAssets(Assets.Create(i % 3 == 0 ? AssetsStatus.Available : AssetsStatus.PreferNotToSay));
            profile.SetGoldStatus(i % 4 == 0);
            profile.UpdateVerification(Verification.Create(
                phoneVerified: i % 3 == 0,
                emailVerified: user.EmailConfirmed,
                identityVerified: i % 3 == 0,
                photoVerified: i % 5 == 0,
                professionVerified: i % 5 == 0,
                educationVerified: i % 5 == 0));

            // 1-in-7 has none (exercises the "only with photos" filter); the rest get a mix of
            // 1-3 photos so multi-photo carousels have something to show, matching the FE's
            // 3-photo cap (see PhotoUploadGrid's MAX_PHOTOS).
            var photoCount = i % 7 == 0 ? 0 : 1 + i % 3;
            profile.ReplacePhotos(Enumerable.Range(0, photoCount)
                .Select(photoIndex => ($"https://picsum.photos/seed/wedproposal{i}-{photoIndex}/600/600", false)));

            profile.UpdateBasics(
                firstName,
                lastName,
                ManagedBy.Self,
                $"Hi, I'm {firstName}! Looking forward to connecting with someone genuine.",
                [HobbyPool[i % HobbyPool.Length], HobbyPool[(i + 3) % HobbyPool.Length], HobbyPool[(i + 6) % HobbyPool.Length]],
                [InterestPool[i % InterestPool.Length], InterestPool[(i + 2) % InterestPool.Length]]);

            dbContext.Profiles.Add(profile);
            seeded.Add((user, profile));
        }

        await SeedRequestsMessagesAndFavouritesAsync(dbContext, demoUser, seeded);

        await dbContext.SaveChangesAsync();
    }

    /// Gives the demo account a mix of connection states, saved profiles, and conversations
    /// (one read, one with an unread reply) so the Requests/Saved/Messages screens have
    /// something to show without the demo user having to click through the app first.
    private static async Task SeedRequestsMessagesAndFavouritesAsync(
        AppDbContext dbContext, ApplicationUser demoUser, List<(ApplicationUser User, Profile Profile)> seeded)
    {
        // Demo sent this one, still pending from the other side.
        dbContext.ProfileConnectionRequests.Add(ProfileConnectionRequest.Create(demoUser.Id, seeded[0].User.Id));

        // Demo sent this one and it was accepted — becomes a connection they can chat with.
        var acceptedFromDemo = ProfileConnectionRequest.Create(demoUser.Id, seeded[1].User.Id);
        acceptedFromDemo.Accept();
        dbContext.ProfileConnectionRequests.Add(acceptedFromDemo);

        // Demo sent this one and it was declined.
        var declinedFromDemo = ProfileConnectionRequest.Create(demoUser.Id, seeded[4].User.Id);
        declinedFromDemo.Decline();
        dbContext.ProfileConnectionRequests.Add(declinedFromDemo);

        // These two sent requests to demo — show up as "pending" in Requests > Received.
        dbContext.ProfileConnectionRequests.Add(ProfileConnectionRequest.Create(seeded[2].User.Id, demoUser.Id));
        dbContext.ProfileConnectionRequests.Add(ProfileConnectionRequest.Create(seeded[3].User.Id, demoUser.Id));

        // A second accepted connection, this time the other party sent it — also chattable.
        var acceptedFromOther = ProfileConnectionRequest.Create(seeded[9].User.Id, demoUser.Id);
        acceptedFromOther.Accept();
        dbContext.ProfileConnectionRequests.Add(acceptedFromOther);

        // Saved profiles.
        dbContext.ProfileFavourites.Add(ProfileFavourite.Create(demoUser.Id, seeded[6].Profile.Id));
        dbContext.ProfileFavourites.Add(ProfileFavourite.Create(demoUser.Id, seeded[7].Profile.Id));
        dbContext.ProfileFavourites.Add(ProfileFavourite.Create(demoUser.Id, seeded[8].Profile.Id));

        // Conversation with seeded[1] (Kavindu) — demo has read up to the second-to-last
        // message, so the other party's latest reply shows as unread.
        var conversationWithKavindu = Conversation.Create(demoUser.Id, seeded[1].User.Id);
        dbContext.Conversations.Add(conversationWithKavindu);
        var kavindu = seeded[1].User;
        dbContext.Messages.AddRange(
            Message.Create(conversationWithKavindu.Id, demoUser.Id, $"Hi {kavindu.FirstName}, I saw your profile and thought we might get along!"),
            Message.Create(conversationWithKavindu.Id, kavindu.Id, "Hey! Thanks for reaching out, nice to meet you."),
            Message.Create(conversationWithKavindu.Id, demoUser.Id, "Likewise! What are you looking for in a partner?"),
            Message.Create(conversationWithKavindu.Id, kavindu.Id, "Someone family-oriented and easy to talk to, mostly :)"));
        conversationWithKavindu.MarkReadFor(demoUser.Id);
        // One more arrives after demo's read point, so it shows up as unread.
        await Task.Delay(5);
        dbContext.Messages.Add(Message.Create(conversationWithKavindu.Id, kavindu.Id, "Are you free for a call sometime this week?"));

        // Conversation with seeded[9] (Nuwan) — fully read, no unread badge.
        var conversationWithNuwan = Conversation.Create(demoUser.Id, seeded[9].User.Id);
        dbContext.Conversations.Add(conversationWithNuwan);
        var nuwan = seeded[9].User;
        dbContext.Messages.AddRange(
            Message.Create(conversationWithNuwan.Id, nuwan.Id, "Hello, thanks for accepting my request!"),
            Message.Create(conversationWithNuwan.Id, demoUser.Id, "Of course! Looking forward to getting to know you."));
        conversationWithNuwan.MarkReadFor(demoUser.Id);

        await dbContext.SaveChangesAsync();
    }
}
