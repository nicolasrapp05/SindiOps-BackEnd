namespace SindiOps.API.Infrastructure.Auth;

public class ConviteResendRateLimitOptions
{
    public const string SectionName = "Auth:ConviteResendRateLimit";

    public int MaxAttemptsPerFuncionario { get; set; } = 3;
    public int WindowMinutes { get; set; } = 5;
}
