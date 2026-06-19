using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Infrastructure.Auth;

public class PasswordResetRateLimiter : IPasswordResetRateLimiter
{
    private readonly IMemoryCache _cache;
    private readonly PasswordResetRateLimitOptions _options;

    public PasswordResetRateLimiter(
        IMemoryCache cache,
        IOptions<PasswordResetRateLimitOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public bool TryAcquire(string email, string clientIp)
    {
        var window = TimeSpan.FromMinutes(Math.Max(1, _options.WindowMinutes));
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var ipKey = string.IsNullOrWhiteSpace(clientIp) ? "unknown" : clientIp.Trim();

        if (!TryAcquireSlot(BuildKey("ip", ipKey), _options.MaxAttemptsPerIp, window))
            return false;

        if (!TryAcquireSlot(BuildKey("email", normalizedEmail), _options.MaxAttemptsPerEmail, window))
            return false;

        return true;
    }

    private bool TryAcquireSlot(string key, int maxAttempts, TimeSpan window)
    {
        if (maxAttempts <= 0)
            return false;

        var entry = _cache.GetOrCreate(key, cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = window;
            return new RateLimitEntry();
        })!;

        lock (entry)
        {
            if (entry.Count >= maxAttempts)
                return false;

            entry.Count++;
            return true;
        }
    }

    private static string BuildKey(string scope, string value) =>
        $"pwd-reset:{scope}:{value}";

    private sealed class RateLimitEntry
    {
        public int Count { get; set; }
    }
}
