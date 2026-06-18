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

    /// <summary>
    /// Funcionário sem vínculos explícitos mantém acesso a todos os condomínios do síndico (legado).
    /// </summary>
    public static async Task<bool> FuncionarioPodeAcessarCondominioAsync(
        SindiOpsDbContext db, Guid userId, Guid sindicoId, Guid condominioId, CancellationToken ct = default)
    {
        if (!await IsFuncionarioDoSindicoAsync(db, userId, sindicoId, ct))
            return await db.Condominios.AsNoTracking()
                .AnyAsync(c => c.Id == condominioId && c.SindicoId == sindicoId, ct);

        var temRestricao = await db.FuncionarioCondominios.AsNoTracking()
            .AnyAsync(fc => fc.FuncionarioId == userId, ct);

        if (!temRestricao)
            return await db.Condominios.AsNoTracking()
                .AnyAsync(c => c.Id == condominioId && c.SindicoId == sindicoId, ct);

        return await db.FuncionarioCondominios.AsNoTracking()
            .AnyAsync(fc => fc.FuncionarioId == userId && fc.CondominioId == condominioId, ct);
    }

    public static async Task<List<Guid>> ObterCondominiosAcessiveisAsync(
        SindiOpsDbContext db, Guid userId, Guid sindicoId, CancellationToken ct = default)
    {
        if (!await IsFuncionarioDoSindicoAsync(db, userId, sindicoId, ct))
        {
            return await db.Condominios.AsNoTracking()
                .Where(c => c.SindicoId == sindicoId)
                .Select(c => c.Id)
                .ToListAsync(ct);
        }

        var restritos = await db.FuncionarioCondominios.AsNoTracking()
            .Where(fc => fc.FuncionarioId == userId)
            .Select(fc => fc.CondominioId)
            .ToListAsync(ct);

        if (restritos.Count == 0)
        {
            return await db.Condominios.AsNoTracking()
                .Where(c => c.SindicoId == sindicoId)
                .Select(c => c.Id)
                .ToListAsync(ct);
        }

        return restritos;
    }
}
