using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SindiOps.API.Constants;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;

namespace SindiOps.API.Services;

public static class UserCargoResolver
{
    public static async Task<string> ResolveAsync(
        SindiOpsDbContext db,
        Guid userId,
        System.Security.Claims.ClaimsPrincipal? user,
        ILogger? logger = null,
        CancellationToken ct = default)
    {
        var cargoDb = await db.Usuarios.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Cargo)
            .FirstOrDefaultAsync(ct);

        if (!string.IsNullOrWhiteSpace(cargoDb))
        {
            logger?.LogDebug("Cargo resolvido via DB (Usuarios) para usuário {UserId}", userId);
            return cargoDb;
        }

        logger?.LogDebug("Cargo resolvido via JWT/claims para usuário {UserId}", userId);
        return ResolveFromClaims(user, logger);
    }

    private static string ResolveFromClaims(
        System.Security.Claims.ClaimsPrincipal? user,
        ILogger? logger)
    {
        if (user is null)
            return string.Empty;

        var direct = user.GetCargo();
        if (!string.IsNullOrWhiteSpace(direct))
            return NormalizeCargo(direct);

        var metadataJson = user.FindFirst("user_metadata")?.Value;
        if (string.IsNullOrWhiteSpace(metadataJson))
            return string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(metadataJson);
            if (doc.RootElement.TryGetProperty("cargo", out var cargoProp))
            {
                var cargo = cargoProp.GetString();
                if (!string.IsNullOrWhiteSpace(cargo))
                    return NormalizeCargo(cargo);
            }
        }
        catch (JsonException ex)
        {
            logger?.LogWarning(ex, "Falha ao parsear user_metadata para resolução de cargo");
        }

        return string.Empty;
    }

    private static string NormalizeCargo(string cargo) =>
        cargo.Trim().ToLowerInvariant() switch
        {
            CargoConstants.Sindico => CargoConstants.Sindico,
            CargoConstants.Secretario => CargoConstants.Secretario,
            CargoConstants.Zelador => CargoConstants.Zelador,
            CargoConstants.Porteiro => CargoConstants.Porteiro,
            CargoConstants.Outro => CargoConstants.Outro,
            _ => string.Empty,
        };
}
