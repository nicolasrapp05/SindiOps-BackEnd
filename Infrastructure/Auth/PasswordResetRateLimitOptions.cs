namespace SindiOps.API.Infrastructure.Auth;

public class PasswordResetRateLimitOptions
{
    public const string SectionName = "Auth:PasswordResetRateLimit";

    public int MaxAttemptsPerEmail { get; set; } = 3;
    public int MaxAttemptsPerIp { get; set; } = 10;
    public int WindowMinutes { get; set; } = 60;
}
