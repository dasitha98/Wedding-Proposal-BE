using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace Wedding_Proposal_BE.Features.Auth.Infrastructure.Services;

/// IMemoryCache-backed implementation. Fine for a single-instance deployment; if the API ever
/// runs behind a load balancer with multiple instances, swap this for an IDistributedCache
/// (e.g. Redis) implementation instead, since a transaction created on one instance would
/// otherwise be invisible to the instance that handles the callback.
public class GoogleOidcTransactionStore : IGoogleOidcTransactionStore
{
    private static readonly TimeSpan TransactionLifetime = TimeSpan.FromMinutes(5);
    private const string CacheKeyPrefix = "google-oidc-txn:";

    private readonly IMemoryCache _cache;

    public GoogleOidcTransactionStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string Create(string codeVerifier, string nonce, string redirectUri)
    {
        var state = GenerateOpaqueToken();
        _cache.Set(CacheKeyPrefix + state, new GoogleOidcTransaction(codeVerifier, nonce, redirectUri), TransactionLifetime);
        return state;
    }

    public bool TryConsume(string state, out GoogleOidcTransaction transaction)
    {
        var key = CacheKeyPrefix + state;
        if (_cache.TryGetValue(key, out GoogleOidcTransaction? found) && found is not null)
        {
            _cache.Remove(key);
            transaction = found;
            return true;
        }

        transaction = null!;
        return false;
    }

    private static string GenerateOpaqueToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
