namespace SindiOps.API.Services.Interfaces;

public interface ICadastroSindicoRateLimiter
{
    bool TryAcquire(string clientIp);
}
