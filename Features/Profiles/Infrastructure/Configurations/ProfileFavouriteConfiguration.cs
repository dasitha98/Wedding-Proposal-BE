using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wedding_Proposal_BE.Features.Profiles.Domain.Entities;

namespace Wedding_Proposal_BE.Features.Profiles.Infrastructure.Configurations;

public class ProfileFavouriteConfiguration : IEntityTypeConfiguration<ProfileFavourite>
{
    public void Configure(EntityTypeBuilder<ProfileFavourite> builder)
    {
        builder.ToTable("ProfileFavourites");

        builder.HasKey(f => f.Id);

        builder.HasIndex(f => new { f.UserId, f.TargetProfileId }).IsUnique();
    }
}
