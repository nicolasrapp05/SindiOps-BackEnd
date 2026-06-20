using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Infrastructure.Auth;

public class ConviteResendRateLimiter : IConviteResendRateLimiter
{
    private readonly IMemoryCache _cache;
    private readonly ConviteResendRateLimitOptions _options;

    public ConviteResendRateLimiter(
        IMemoryCache cache,
        IOptions<ConviteResendRateLimitOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public bool TryAcquire(Guid funcionarioId)
    {
        var window = TimeSpan.FromMinutes(Math.Max(1, _options.WindowMinutes));
        var maxAttempts = Math.Max(1, _options.MaxAttemptsPerFuncionario);
        var key = $"convite-resend:{funcionarioId}";

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

    private sealed class RateLimitEntry
    {
        public int Count { get; set; }
    }
}
