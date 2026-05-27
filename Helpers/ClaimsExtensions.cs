using System.Security.Claims;

namespace SindiOps.API.Helpers;

public static class ClaimsExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var id))
            throw new UnauthorizedAccessException("Token inválido ou ausente");

        return id;
    }

    public static string GetCargo(this ClaimsPrincipal user)
    {
        return user.FindFirst("cargo")?.Value ?? string.Empty;
    }
}
