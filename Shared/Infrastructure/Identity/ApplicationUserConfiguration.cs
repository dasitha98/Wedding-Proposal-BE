using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wedding_Proposal_BE.Shared.Infrastructure.Identity;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.GoogleSubject).HasMaxLength(255);
        builder.Property(u => u.Name).HasMaxLength(255);
        builder.Property(u => u.ProfileImageUrl).HasMaxLength(2048);

        // Filtered unique index: many users have no Google identity (null), and Postgres
        // treats distinct NULLs as non-equal, so a plain unique index already allows any
        // number of nulls — the filter just documents that intent and matches other DBs.
        builder.HasIndex(u => u.GoogleSubject)
            .IsUnique()
            .HasFilter("\"GoogleSubject\" IS NOT NULL");
    }
}
