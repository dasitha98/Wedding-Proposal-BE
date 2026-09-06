using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Wedding_Proposal_BE.Features.Auth.Infrastructure.Options;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

/// Google's token endpoint, per https://developers.google.com/identity/protocols/oauth2/web-server#exchange-authorization-code.
/// A plain, documented REST call — no cryptography happens here; ID token verification (the
/// crypto-sensitive part) is delegated entirely to Google.Apis.Auth's GoogleJsonWebSignature.
file record GoogleTokenResponse(
    [property: JsonPropertyName("id_token")] string? IdToken,
    [property: JsonPropertyName("error")] string? Error,
    [property: JsonPropertyName("error_description")] string? ErrorDescription);

public class GoogleOidcClient : IGoogleOidcClient
{
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";

    private readonly HttpClient _httpClient;
    private readonly GoogleAuthOptions _options;

    public GoogleOidcClient(HttpClient httpClient, IOptions<GoogleAuthOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<GoogleIdentity> AuthenticateAsync(
        string code, string codeVerifier, string redirectUri, string expectedNonce, CancellationToken cancellationToken)
    {
        var idToken = await ExchangeCodeForIdTokenAsync(code, codeVerifier, redirectUri, cancellationToken);

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_options.ClientId]
            });
        }
        catch (InvalidJwtException ex)
        {
            throw new GoogleOidcException("Google sign-in failed.", ex);
        }

        // GoogleJsonWebSignature.ValidateAsync already checks signature, issuer, audience, and
        // expiry — nonce binding is app-specific, so it's checked here instead.
        if (string.IsNullOrEmpty(payload.Nonce) || payload.Nonce != expectedNonce)
        {
            throw new GoogleOidcException("Google sign-in failed.");
        }

        if (string.IsNullOrEmpty(payload.Subject) || string.IsNullOrEmpty(payload.Email))
        {
            throw new GoogleOidcException("Google sign-in failed.");
        }

        return new GoogleIdentity(payload.Subject, payload.Email, payload.EmailVerified, payload.Name, payload.Picture);
    }

    private async Task<string> ExchangeCodeForIdTokenAsync(
        string code, string codeVerifier, string redirectUri, CancellationToken cancellationToken)
    {
        var requestBody = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = codeVerifier
        };

        using var response = await _httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(requestBody), cancellationToken);
        var token = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode || token is null || string.IsNullOrEmpty(token.IdToken))
        {
            // Deliberately not logging `requestBody` or the response body: both can carry the
            // authorization code / client secret, which must never end up in logs.
            throw new GoogleOidcException($"Google token exchange failed ({(int)response.StatusCode}).");
        }

        return token.IdToken;
    }
}
