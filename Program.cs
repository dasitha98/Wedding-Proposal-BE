using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Admin;
using Wedding_Proposal_BE.Features.Auth;
using Wedding_Proposal_BE.Features.Discover;
using Wedding_Proposal_BE.Features.Messages;
using Wedding_Proposal_BE.Features.Profiles;
using Wedding_Proposal_BE.Features.Users;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;
using Wedding_Proposal_BE.Shared.Infrastructure.Storage;
using Wedding_Proposal_BE.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Data
// Falls back to an in-memory database when no real connection string is configured yet, so
// the API is runnable end-to-end before Postgres is set up. Set ConnectionStrings:
// DefaultConnection (appsettings.Development.json) to switch to real Postgres — data in the
// in-memory DB does not persist across restarts and migrations are not applied to it.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// "UseInMemoryDatabase" (appsettings.Development.json) lets you force the in-memory DB even
// when a real connection string is already configured (e.g. via user-secrets) but Postgres
// itself isn't up yet. Remove/set it to false once your Postgres database exists.
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase")
    || string.IsNullOrWhiteSpace(connectionString);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useInMemoryDatabase)
    {
        options.UseInMemoryDatabase("WeddingProposalDev");
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});

// Identity
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 0;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

// The Expo web dev server runs on its own origin (e.g. http://localhost:8081), so browser
// requests to this API need CORS. Wide open in Development only — never in production.
const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
        policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

// Rate limiting: brute-force/spam protection on login and register
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});

// Feature registrations (each feature owns its own DI wiring)
builder.Services.AddAuthFeature(builder.Configuration);
builder.Services.AddUsersFeature();
builder.Services.AddProfilesFeature();
builder.Services.AddDiscoverFeature();
builder.Services.AddMessagesFeature();
builder.Services.AddAdminFeature();
builder.Services.AddObjectStorage(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums serialize as their FE-matching camelCase string (e.g. "neverMarried"), not
        // numbers — see ProfileEnums.cs for the few members that need an explicit override.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });
builder.Services.AddOpenApi();

var app = builder.Build();

if (useInMemoryDatabase)
{
    app.Logger.LogWarning(
        "No ConnectionStrings:DefaultConnection configured — using an in-memory database. " +
        "Data will not persist across restarts. Set a real Postgres connection string and run " +
        "'dotnet ef database update' to switch over.");

    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    await DevSeedData.SeedAsync(scope.ServiceProvider);
}
else if (builder.Configuration.GetValue<bool>("SeedDatabase"))
{
    // Opt-in, one-off seeding of a real (e.g. Postgres) database with the same dev sample
    // data used for the in-memory DB. Set "SeedDatabase": true only to run this once, then
    // turn it back off — it does not guard against re-seeding on every startup.
    using var scope = app.Services.CreateScope();
    await DevSeedData.SeedAsync(scope.ServiceProvider);
}

// Always ensures the three core roles (SuperAdmin, Admin, User) exist, and seeds one admin
// account from AdminSeed:Email/Password (appsettings.json, override via user-secrets) if it
// doesn't exist yet — idempotent, safe to run on every startup unlike the opt-in
// DevSeedData/SeedDatabase paths above.
using (var seedScope = app.Services.CreateScope())
{
    var roleManager = seedScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    foreach (var roleName in RoleNames.All)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName) { Id = Guid.NewGuid() });
        }
    }

    // Backfills the "User" role onto any account created before role assignment existed —
    // a no-op once every user has at least one role.
    var seedDbContext = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userRoleId = (await roleManager.FindByNameAsync(RoleNames.User))!.Id;
    var userIdsWithoutRoles = await seedDbContext.Users
        .Where(u => !seedDbContext.UserRoles.Any(ur => ur.UserId == u.Id))
        .Select(u => u.Id)
        .ToListAsync();
    if (userIdsWithoutRoles.Count > 0)
    {
        seedDbContext.UserRoles.AddRange(
            userIdsWithoutRoles.Select(userId => new IdentityUserRole<Guid> { UserId = userId, RoleId = userRoleId }));
        await seedDbContext.SaveChangesAsync();
    }

    var adminEmail = builder.Configuration["AdminSeed:Email"];
    var adminPassword = builder.Configuration["AdminSeed:Password"];
    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var userManager = seedScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = ApplicationUser.Create(adminEmail, "Admin", "User");
            adminUser.EmailConfirmed = true;
            await userManager.CreateAsync(adminUser, adminPassword);
        }

        if (!await userManager.IsInRoleAsync(adminUser, RoleNames.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, RoleNames.Admin);
        }
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors(DevCorsPolicy);

app.UseRateLimiter();

app.UseAuthentication();
app.UseMiddleware<LastActiveTrackingMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
