using System.Security.Cryptography;
using System.Text;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

/// Shared by RegistrationService and PasswordService — both issue and verify hashed,
/// numeric one-time codes and would otherwise duplicate this identically.
public static class OtpCodeGenerator
{
    public static string GenerateCode() => RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

    public static string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }
}
