namespace SindiOps.API.Services.Interfaces;

public interface IPasswordResetRateLimiter
{
    bool TryAcquire(string email, string clientIp);
}
