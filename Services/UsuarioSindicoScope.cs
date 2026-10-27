using Microsoft.EntityFrameworkCore;
using SindiOps.API.Infrastructure.Data;

namespace SindiOps.API.Services;

/// <summary>
/// Resolve a carteira (<c>sindicos.id</c>) a partir do usuário autenticado (<c>usuarios.id</c>).
/// </summary>
public static class UsuarioSindicoScope
{
    public static async Task<Guid> ResolveSindicoIdAsync(
        SindiOpsDbContext db, Guid userId, CancellationToken ct = default)
    {
        var papeis = await db.Usuarios.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                SindicoId = u.Sindico != null ? (Guid?)u.Sindico.Id : null,
                EmpregadorId = u.Funcionario != null ? (Guid?)u.Funcionario.SindicoId : null,
            })
            .FirstOrDefaultAsync(ct);

        if (papeis is null)
            throw new UnauthorizedAccessException("Usuário não autorizado");

        if (papeis.SindicoId is Guid proprio)
            return proprio;

        if (papeis.EmpregadorId is Guid empregador)
            return empregador;

        throw new UnauthorizedAccessException("Usuário não autorizado");
    }

    public static Task<bool> IsFuncionarioDoSindicoAsync(
        SindiOpsDbContext db, Guid userId, Guid sindicoId, CancellationToken ct = default) =>
        db.Funcionarios.AnyAsync(
            f => f.UsuarioId == userId
                 && f.SindicoId == sindicoId
                 && (f.Usuario.Sindico == null || f.Usuario.Sindico.Id != sindicoId),
            ct);

    /// <summary>Token da equipe do síndico ou token do próprio síndico.</summary>
    public static async Task<bool> IsFuncionarioOuSindicoPrincipalAsync(
        SindiOpsDbContext db, Guid userId, Guid sindicoId, CancellationToken ct = default)
    {
        if (await IsFuncionarioDoSindicoAsync(db, userId, sindicoId, ct))
            return true;

        return await db.Sindicos.AsNoTracking()
            .AnyAsync(s => s.UsuarioId == userId && s.Id == sindicoId, ct);
    }

    public static Task<bool> EmailDeLoginEmUsoAsync(
        SindiOpsDbContext db, string emailNormalizado, Guid? ignorarUsuarioId = null, CancellationToken ct = default)
    {
        var query = db.Usuarios.AsNoTracking()
            .Where(u =>
                u.Email.ToLower() == emailNormalizado
                && (u.Sindico != null || u.Funcionario != null));

        if (ignorarUsuarioId is Guid id)
            query = query.Where(u => u.Id != id);

        return query.AnyAsync(ct);
    }

    /// <summary>
    /// Funcionário sem vínculos explícitos mantém acesso a todos os condomínios do síndico.
    /// </summary>
    public static async Task<bool> FuncionarioPodeAcessarCondominioAsync(
        SindiOpsDbContext db, Guid userId, Guid sindicoId, Guid condominioId, CancellationToken ct = default)
    {
        var funcionarioId = await IdFuncionarioRestritoAsync(db, userId, sindicoId, ct);

        if (funcionarioId is null)
            return await db.Condominios.AsNoTracking()
                .AnyAsync(c => c.Id == condominioId && c.SindicoId == sindicoId, ct);

        var temRestricao = await db.FuncionarioCondominios.AsNoTracking()
            .AnyAsync(fc => fc.FuncionarioId == funcionarioId, ct);

        if (!temRestricao)
            return await db.Condominios.AsNoTracking()
                .AnyAsync(c => c.Id == condominioId && c.SindicoId == sindicoId, ct);

        return await db.FuncionarioCondominios.AsNoTracking()
            .AnyAsync(fc => fc.FuncionarioId == funcionarioId && fc.CondominioId == condominioId, ct);
    }

    public static async Task<List<Guid>> ObterCondominiosAcessiveisAsync(
        SindiOpsDbContext db, Guid userId, Guid sindicoId, CancellationToken ct = default)
    {
        var funcionarioId = await IdFuncionarioRestritoAsync(db, userId, sindicoId, ct);

        if (funcionarioId is null)
        {
            return await db.Condominios.AsNoTracking()
                .Where(c => c.SindicoId == sindicoId)
                .Select(c => c.Id)
                .ToListAsync(ct);
        }

        var restritos = await db.FuncionarioCondominios.AsNoTracking()
            .Where(fc => fc.FuncionarioId == funcionarioId)
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

    private static Task<Guid?> IdFuncionarioRestritoAsync(
        SindiOpsDbContext db, Guid userId, Guid sindicoId, CancellationToken ct) =>
        db.Funcionarios.AsNoTracking()
            .Where(f =>
                f.UsuarioId == userId
                && f.SindicoId == sindicoId
                && (f.Usuario.Sindico == null || f.Usuario.Sindico.Id != sindicoId))
            .Select(f => (Guid?)f.Id)
            .FirstOrDefaultAsync(ct);
}
