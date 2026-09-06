using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wedding_Proposal_BE.Features.Auth.Domain.Entities;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Configurations;

public class PasswordResetOtpConfiguration : IEntityTypeConfiguration<PasswordResetOtp>
{
    public void Configure(EntityTypeBuilder<PasswordResetOtp> builder)
    {
        builder.ToTable("PasswordResetOtps");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Email).IsRequired().HasMaxLength(256);
        builder.Property(o => o.CodeHash).IsRequired().HasMaxLength(128);

        builder.HasIndex(o => o.Email).IsUnique();
    }
}
