using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Features.Auth.Domain.Entities;
using Wedding_Proposal_BE.Features.Messages.Domain.Entities;
using Wedding_Proposal_BE.Features.Profiles.Domain.Entities;
using Wedding_Proposal_BE.Shared.Infrastructure.Identity;

namespace Wedding_Proposal_BE.Shared.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetOtp> PasswordResetOtps => Set<PasswordResetOtp>();
    public DbSet<EmailVerificationOtp> EmailVerificationOtps => Set<EmailVerificationOtp>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<ProfileFavourite> ProfileFavourites => Set<ProfileFavourite>();
    public DbSet<ProfileConnectionRequest> ProfileConnectionRequests => Set<ProfileConnectionRequest>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Identity defaults these to "AspNet"-prefixed table names; rename to plain names.
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");

        // Claims, external logins and persisted auth tokens are never used by this app (no
        // claim-based permissions, Google sign-in is a custom OIDC flow rather than Identity's
        // external-login mechanism, and password-reset tokens are stateless) — drop the tables.
        builder.Ignore<IdentityUserClaim<Guid>>();
        builder.Ignore<IdentityRoleClaim<Guid>>();
        builder.Ignore<IdentityUserLogin<Guid>>();
        builder.Ignore<IdentityUserToken<Guid>>();
    }
}
