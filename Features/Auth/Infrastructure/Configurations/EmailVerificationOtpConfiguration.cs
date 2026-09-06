using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wedding_Proposal_BE.Features.Auth.Domain.Entities;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Configurations;

public class EmailVerificationOtpConfiguration : IEntityTypeConfiguration<EmailVerificationOtp>
{
    public void Configure(EntityTypeBuilder<EmailVerificationOtp> builder)
    {
        builder.ToTable("EmailVerificationOtps");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Email).IsRequired().HasMaxLength(256);
        builder.Property(o => o.CodeHash).IsRequired().HasMaxLength(128);

        builder.HasIndex(o => o.Email).IsUnique();
    }
}
