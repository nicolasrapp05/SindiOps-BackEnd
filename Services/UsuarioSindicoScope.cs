using Microsoft.EntityFrameworkCore;
using SindiOps.API.Infrastructure.Data;

namespace SindiOps.API.Services;

/// <summary>Resolve o <c>sindico_id</c> de escopo a partir do <c>sub</c> (síndico ou funcionário).</summary>
public static class UsuarioSindicoScope
{
    public static async Task<Guid> ResolveSindicoIdAsync(SindiOpsDbContext db, Guid userId, CancellationToken ct = default)
    {
        var sindicoDoFuncionario = await db.Funcionarios.AsNoTracking()
            .Where(f => f.Id == userId)
            .Select(f => f.SindicoId)
            .FirstOrDefaultAsync(ct);

        if (sindicoDoFuncionario != Guid.Empty)
            return sindicoDoFuncionario;

        if (await db.Sindicos.AsNoTracking().AnyAsync(s => s.Id == userId, ct))
            return userId;

        throw new UnauthorizedAccessException("Usuário não autorizado");
    }

    public static Task<bool> IsFuncionarioDoSindicoAsync(
        SindiOpsDbContext db, Guid userId, Guid sindicoId, CancellationToken ct = default) =>
        db.Funcionarios.AnyAsync(f => f.Id == userId && f.SindicoId == sindicoId, ct);

    /// <summary>Token de funcionário do síndico ou token do próprio síndico (<paramref name="userId"/> == <paramref name="sindicoId"/>).</summary>
    public static async Task<bool> IsFuncionarioOuSindicoPrincipalAsync(
        SindiOpsDbContext db, Guid userId, Guid sindicoId, CancellationToken ct = default)
    {
        if (await IsFuncionarioDoSindicoAsync(db, userId, sindicoId, ct))
            return true;
        return userId == sindicoId && await db.Sindicos.AsNoTracking().AnyAsync(s => s.Id == userId, ct);
    }
}
