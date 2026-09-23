using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.Infrastructure.Data;

namespace SindiOps.API.Services;

/// <summary>Resolve o <c>sindico_id</c> de escopo a partir do <c>sub</c> em <c>usuarios</c>.</summary>
public static class UsuarioSindicoScope
{
    public static async Task<Guid> ResolveSindicoIdAsync(SindiOpsDbContext db, Guid userId, CancellationToken ct = default)
    {
        var usuario = await db.Usuarios.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.Cargo, u.SindicoId })
            .FirstOrDefaultAsync(ct);

        if (usuario is null)
            throw new UnauthorizedAccessException("Usuário não autorizado");

        if (usuario.Cargo == CargoConstants.Sindico)
            return userId;

        if (usuario.SindicoId is Guid sindicoId)
            return sindicoId;

        throw new UnauthorizedAccessException("Usuário não autorizado");
    }

    public static Task<bool> IsFuncionarioDoSindicoAsync(
        SindiOpsDbContext db, Guid userId, Guid sindicoId, CancellationToken ct = default) =>
        db.Usuarios.AnyAsync(
            u => u.Id == userId && u.SindicoId == sindicoId && u.Cargo != CargoConstants.Sindico,
            ct);

    /// <summary>Token da equipe do síndico ou token do próprio síndico.</summary>
    public static async Task<bool> IsFuncionarioOuSindicoPrincipalAsync(
        SindiOpsDbContext db, Guid userId, Guid sindicoId, CancellationToken ct = default)
    {
        if (await IsFuncionarioDoSindicoAsync(db, userId, sindicoId, ct))
            return true;

        return await db.Usuarios.AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.Id == sindicoId && u.Cargo == CargoConstants.Sindico, ct);
    }

    public static Task<bool> EmailDeLoginEmUsoAsync(
        SindiOpsDbContext db, string emailNormalizado, Guid? ignorarUsuarioId = null, CancellationToken ct = default)
    {
        var query = db.Usuarios.AsNoTracking()
            .Where(u => u.Pessoa.Email.ToLower() == emailNormalizado);

        if (ignorarUsuarioId is Guid id)
            query = query.Where(u => u.Id != id);

        return query.AnyAsync(ct);
    }

    /// <summary>
    /// Usuário sem vínculos explícitos mantém acesso a todos os condomínios do síndico.
    /// </summary>
    public static async Task<bool> FuncionarioPodeAcessarCondominioAsync(
        SindiOpsDbContext db, Guid userId, Guid sindicoId, Guid condominioId, CancellationToken ct = default)
    {
        if (!await IsFuncionarioDoSindicoAsync(db, userId, sindicoId, ct))
            return await db.Condominios.AsNoTracking()
                .AnyAsync(c => c.Id == condominioId && c.SindicoId == sindicoId, ct);

        var temRestricao = await db.UsuarioCondominios.AsNoTracking()
            .AnyAsync(uc => uc.UsuarioId == userId, ct);

        if (!temRestricao)
            return await db.Condominios.AsNoTracking()
                .AnyAsync(c => c.Id == condominioId && c.SindicoId == sindicoId, ct);

        return await db.UsuarioCondominios.AsNoTracking()
            .AnyAsync(uc => uc.UsuarioId == userId && uc.CondominioId == condominioId, ct);
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

        var restritos = await db.UsuarioCondominios.AsNoTracking()
            .Where(uc => uc.UsuarioId == userId)
            .Select(uc => uc.CondominioId)
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
