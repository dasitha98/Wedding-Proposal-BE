using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Wedding_Proposal_BE.Features.Auth.Application.Interfaces;
using Wedding_Proposal_BE.Features.Auth.Infrastructure.Options;
using Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

namespace Wedding_Proposal_BE.Features.Auth;

/// Registers everything the Auth feature owns: JWT bearer authentication, options binding,
/// and its scoped services. Keeps Program.cs a thin composition root.
public static class AuthServiceRegistration
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddScoped<TokenClaimsFactory>();
        services.AddScoped<ITokenService, TokenService>();

        // Delivery for OTP codes goes through the Notification feature's IEmailSender —
        // registered by AddNotificationFeature, which must run before AddAuthFeature.
        services.AddScoped<IOtpSender, EmailOtpSender>();

        // Shared by every use case below that issues an authenticated session.
        services.AddScoped<IAuthResponseFactory, AuthResponseFactory>();

        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IGoogleSignInService, GoogleSignInService>();

        // Google OIDC sign-in: a memory cache for the short-lived, single-use PKCE/nonce
        // transactions (see GoogleOidcTransactionStore for why this must become a distributed
        // cache if the API ever runs as more than one instance), and a typed HttpClient for
        // the authorization-code exchange call to Google's token endpoint.
        services.AddMemoryCache();
        services.AddSingleton<IGoogleOidcTransactionStore, GoogleOidcTransactionStore>();
        services.AddHttpClient<IGoogleOidcClient, GoogleOidcClient>();

        return services;
    }
}
