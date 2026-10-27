using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.Entities;
using SindiOps.API.Infrastructure.Data;

namespace SindiOps.API.Services;

public static class UsuarioPapeis
{
    public static string Cargo(Usuario? usuario)
    {
        if (usuario is null)
            return string.Empty;

        if (usuario.Sindico is not null)
            return CargoConstants.Sindico;

        return usuario.Funcionario?.Cargo ?? string.Empty;
    }

    public static bool TemAcesso(Usuario usuario) =>
        usuario.Sindico is not null || usuario.Funcionario is not null;

    public static async Task CarregarAsync(
        SindiOpsDbContext db, Usuario usuario, CancellationToken ct = default)
    {
        await db.Entry(usuario).Reference(u => u.Sindico).LoadAsync(ct);
        await db.Entry(usuario).Reference(u => u.Funcionario).LoadAsync(ct);
    }
}
