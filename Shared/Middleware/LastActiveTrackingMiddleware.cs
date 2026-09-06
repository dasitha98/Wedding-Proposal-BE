using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wedding_Proposal_BE.Shared.Infrastructure.Data;

namespace Wedding_Proposal_BE.Shared.Middleware;

/// Stamps ApplicationUser.LastActiveAt on authenticated requests, throttled so a busy user
/// doesn't cause a write on every single call (Middleware Pipeline Pattern).
public class LastActiveTrackingMiddleware
{
    private static readonly TimeSpan Throttle = TimeSpan.FromMinutes(10);

    private readonly RequestDelegate _next;
    private readonly ILogger<LastActiveTrackingMiddleware> _logger;

    public LastActiveTrackingMiddleware(RequestDelegate next, ILogger<LastActiveTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        await _next(context);

        if (context.User.Identity?.IsAuthenticated != true) return;

        var subject = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out var userId)) return;

        try
        {
            var staleBefore = DateTime.UtcNow - Throttle;
            await dbContext.Users
                .Where(u => u.Id == userId && (u.LastActiveAt == null || u.LastActiveAt < staleBefore))
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastActiveAt, DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record last-active timestamp for user {UserId}", userId);
        }
    }
}
