namespace SindiOps.API.Infrastructure.Auth;

public class CadastroSindicoRateLimitOptions
{
    public const string SectionName = "Auth:CadastroSindicoRateLimit";

    public int MaxAttemptsPerIp { get; set; } = 5;
    public int WindowMinutes { get; set; } = 60;
}
