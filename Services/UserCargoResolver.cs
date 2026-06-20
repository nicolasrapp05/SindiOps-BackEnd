using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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
        CancellationToken ct = default)
    {
        if (await db.Sindicos.AsNoTracking().AnyAsync(s => s.Id == userId, ct))
            return CargoConstants.Sindico;

        var cargoFuncionario = await db.Funcionarios.AsNoTracking()
            .Where(f => f.Id == userId)
            .Select(f => f.Cargo)
            .FirstOrDefaultAsync(ct);

        if (!string.IsNullOrWhiteSpace(cargoFuncionario))
            return cargoFuncionario;

        return ResolveFromClaims(user);
    }

    private static string ResolveFromClaims(System.Security.Claims.ClaimsPrincipal? user)
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
        catch (JsonException)
        {
            /* fallback vazio */
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
