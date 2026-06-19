namespace SindiOps.API.Infrastructure.Auth;

public record RecoveryLinkData(string EmailOtp, string HashedToken);
