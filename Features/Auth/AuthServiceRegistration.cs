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
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

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

        // Real SMTP delivery once Smtp:Username/Password are configured (e.g. via user-secrets);
        // otherwise fall back to logging the code so the app still runs without email set up.
        var smtpOptions = configuration.GetSection(SmtpOptions.SectionName).Get<SmtpOptions>();
        if (!string.IsNullOrWhiteSpace(smtpOptions?.Username) && !string.IsNullOrWhiteSpace(smtpOptions.Password))
        {
            services.AddScoped<IOtpSender, SmtpOtpSender>();
        }
        else
        {
            services.AddScoped<IOtpSender, LoggingOtpSender>();
        }

        services.AddScoped<IAuthService, AuthService>();

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
