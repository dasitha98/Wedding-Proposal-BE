using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wedding_Proposal_BE.Features.Profiles.Domain.Entities;

namespace Wedding_Proposal_BE.Features.Profiles.Infrastructure.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(50);
        builder.Property(p => p.AboutMe).HasMaxLength(1000);
        builder.Property(p => p.Hobbies).HasColumnType("text[]");
        builder.Property(p => p.Interests).HasColumnType("text[]");

        builder.HasIndex(p => p.UserId).IsUnique();

        builder.OwnsOne(p => p.BasicInfo, b =>
        {
            b.Property(x => x.Race).HasMaxLength(100);
            b.Property(x => x.Religion).HasMaxLength(100);
            b.Property(x => x.Caste).HasMaxLength(100);
            b.Property(x => x.HeightLabel).HasMaxLength(20);
        });

        builder.OwnsOne(p => p.Education);

        builder.OwnsOne(p => p.Profession, b =>
        {
            b.Property(x => x.Occupation).HasMaxLength(150);
        });

        builder.OwnsOne(p => p.Residency, b =>
        {
            b.Property(x => x.City).HasMaxLength(100);
            b.Property(x => x.District).HasMaxLength(100);
            b.Property(x => x.Country).HasMaxLength(100);
        });

        builder.OwnsOne(p => p.Family, b =>
        {
            b.Property(x => x.FatherOccupation).HasMaxLength(150);
            b.Property(x => x.MotherOccupation).HasMaxLength(150);
        });

        builder.OwnsOne(p => p.Lifestyle);
        builder.OwnsOne(p => p.Assets);
        builder.OwnsOne(p => p.Verification);

        builder.HasMany(p => p.Siblings)
            .WithOne()
            .HasForeignKey(s => s.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Siblings).HasField("_siblings").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Photos)
            .WithOne()
            .HasForeignKey(ph => ph.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Photos).HasField("_photos").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
