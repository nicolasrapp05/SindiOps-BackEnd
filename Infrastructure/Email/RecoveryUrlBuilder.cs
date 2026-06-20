namespace SindiOps.API.Infrastructure.Email;

public static class RecoveryUrlBuilder
{
    public static string Build(string frontendUrl, string path, string hashedToken)
    {
        var baseUrl = $"{frontendUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        if (string.IsNullOrWhiteSpace(hashedToken))
            return baseUrl;

        return $"{baseUrl}?token_hash={Uri.EscapeDataString(hashedToken)}&type=recovery";
    }
}
