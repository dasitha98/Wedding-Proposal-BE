using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Wedding_Proposal_BE.Features.Notification.EmailNotification.Application.Interfaces;
using Wedding_Proposal_BE.Features.Notification.EmailNotification.Infrastructure.Options;

namespace Wedding_Proposal_BE.Features.Notification.EmailNotification.Infrastructure.Services;

/// Sends email through the SendPulse SMTP service via its REST API
/// (POST /smtp/emails, authenticated with an OAuth client-credentials token).
public class SendPulseEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly SendPulseOptions _options;
    private readonly SendPulseTokenCache _tokenCache;
    private readonly ILogger<SendPulseEmailSender> _logger;

    public SendPulseEmailSender(
        HttpClient http,
        IOptions<SendPulseOptions> options,
        SendPulseTokenCache tokenCache,
        ILogger<SendPulseEmailSender> logger)
    {
        _http = http;
        _options = options.Value;
        _tokenCache = tokenCache;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body)
    {
        var html = "<html><body><p>" +
                   WebUtility.HtmlEncode(body).Replace("\r\n", "\n").Replace("\n", "<br/>") +
                   "</p></body></html>";

        var payload = new SendEmailRequest(new EmailPayload(
            Html: Convert.ToBase64String(Encoding.UTF8.GetBytes(html)),
            Text: body,
            Subject: subject,
            From: new Address(_options.FromName, _options.FromEmail),
            To: [new Address(null, toEmail)]));

        var response = await PostEmailAsync(payload, await GetTokenAsync(forceRefresh: false));

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Token may have been revoked/expired early — refresh once and retry.
            response.Dispose();
            response = await PostEmailAsync(payload, await GetTokenAsync(forceRefresh: true));
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"SendPulse failed to send email ({(int)response.StatusCode}): {error}");
            }
        }

        _logger.LogInformation("Email sent to {Email} via SendPulse", toEmail);
    }

    private Task<HttpResponseMessage> PostEmailAsync(SendEmailRequest payload, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "smtp/emails")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _http.SendAsync(request);
    }

    private async Task<string> GetTokenAsync(bool forceRefresh)
    {
        if (!forceRefresh && _tokenCache.IsValid)
        {
            return _tokenCache.AccessToken!;
        }

        await _tokenCache.Lock.WaitAsync();
        try
        {
            if (forceRefresh)
            {
                _tokenCache.Invalidate();
            }
            else if (_tokenCache.IsValid)
            {
                return _tokenCache.AccessToken!;
            }

            using var response = await _http.PostAsJsonAsync("oauth/access_token", new
            {
                grant_type = "client_credentials",
                client_id = _options.ClientId,
                client_secret = _options.ClientSecret
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"SendPulse authentication failed ({(int)response.StatusCode}): {error}");
            }

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>()
                        ?? throw new InvalidOperationException("SendPulse returned an empty token response.");

            _tokenCache.AccessToken = token.AccessToken;
            // Refresh a minute early to avoid using a token right as it expires.
            _tokenCache.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(token.ExpiresIn - 60, 0));
            return token.AccessToken;
        }
        finally
        {
            _tokenCache.Lock.Release();
        }
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record SendEmailRequest(
        [property: JsonPropertyName("email")] EmailPayload Email);

    private sealed record EmailPayload(
        [property: JsonPropertyName("html")] string Html,
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("from")] Address From,
        [property: JsonPropertyName("to")] Address[] To);

    private sealed record Address(
        [property: JsonPropertyName("name"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Name,
        [property: JsonPropertyName("email")] string Email);
}
