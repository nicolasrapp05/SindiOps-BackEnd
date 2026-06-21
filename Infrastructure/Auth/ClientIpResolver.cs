namespace SindiOps.API.Infrastructure.Auth;

public static class ClientIpResolver
{
    public static string Resolve(HttpContext? context, IConfiguration? configuration = null)
    {
        if (context is null)
            return "unknown";

        var trustForwarded = configuration?
            .GetSection(ForwardedHeadersSettings.SectionName)
            .GetValue<bool>("Enabled") ?? false;

        if (trustForwarded)
        {
            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
                return forwarded.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.MapToIPv4().ToString()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
    }
}
