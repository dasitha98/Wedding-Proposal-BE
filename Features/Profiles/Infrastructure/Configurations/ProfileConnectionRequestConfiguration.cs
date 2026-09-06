using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wedding_Proposal_BE.Features.Profiles.Domain.Entities;

namespace Wedding_Proposal_BE.Features.Profiles.Infrastructure.Configurations;

public class ProfileConnectionRequestConfiguration : IEntityTypeConfiguration<ProfileConnectionRequest>
{
    public void Configure(EntityTypeBuilder<ProfileConnectionRequest> builder)
    {
        builder.ToTable("ProfileConnectionRequests");

        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new { r.RequesterUserId, r.TargetUserId }).IsUnique();
    }
}
