namespace SindiOps.API.Services.Interfaces;

public interface IConviteResendRateLimiter
{
    bool TryAcquire(Guid funcionarioId);
}
