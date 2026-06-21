using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Infrastructure.Auth;

public class CadastroSindicoRateLimiter : ICadastroSindicoRateLimiter
{
    private readonly IMemoryCache _cache;
    private readonly CadastroSindicoRateLimitOptions _options;

    public CadastroSindicoRateLimiter(
        IMemoryCache cache,
        IOptions<CadastroSindicoRateLimitOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public bool TryAcquire(string clientIp)
    {
        var window = TimeSpan.FromMinutes(Math.Max(1, _options.WindowMinutes));
        var ipKey = string.IsNullOrWhiteSpace(clientIp) ? "unknown" : clientIp.Trim();
        return TryAcquireSlot(BuildKey(ipKey), _options.MaxAttemptsPerIp, window);
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

    private static string BuildKey(string value) => $"cadastro-sindico:ip:{value}";

    private sealed class RateLimitEntry
    {
        public int Count { get; set; }
    }
}
