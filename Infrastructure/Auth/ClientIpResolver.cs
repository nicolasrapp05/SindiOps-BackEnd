namespace SindiOps.API.Infrastructure.Auth;

public static class ClientIpResolver
{
    public static string Resolve(HttpContext? context)
    {
        if (context is null)
            return "unknown";

        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.MapToIPv4().ToString()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
    }
}
