using Microsoft.EntityFrameworkCore;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class PerfilService : IPerfilService
{
    private readonly SindiOpsDbContext _db;
    private readonly ISupabaseAuthService _supabaseAuth;

    public PerfilService(SindiOpsDbContext db, ISupabaseAuthService supabaseAuth)
    {
        _db = db;
        _supabaseAuth = supabaseAuth;
    }

    public async Task<PerfilResponse> GetMeAsync(Guid userId)
    {
        var response = await LoadPerfilAsync(userId);
        await _supabaseAuth.SyncUserMetadataAsync(response.Id, response.Nome, response.Cargo);
        return response;
    }

    public async Task<PerfilResponse> UpdateMeAsync(Guid userId, UpdatePerfilRequest request)
    {
        var nome = request.Nome.Trim();

        var usuario = await _db.Usuarios
            .Include(u => u.Pessoa)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedAccessException("Usuário não encontrado");

        usuario.Pessoa.Nome = nome;
        usuario.Pessoa.AtualizadoEm = DateTime.UtcNow;
        usuario.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var response = Map(usuario);
        await _supabaseAuth.SyncUserMetadataAsync(userId, response.Nome, response.Cargo);
        return response;
    }

    private async Task<PerfilResponse> LoadPerfilAsync(Guid userId)
    {
        var usuario = await _db.Usuarios.AsNoTracking()
            .Include(u => u.Pessoa)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedAccessException("Usuário não encontrado");

        return Map(usuario);
    }

    private static PerfilResponse Map(Entities.Usuario usuario) => new()
    {
        Id = usuario.Id,
        Nome = usuario.Pessoa.Nome,
        Email = usuario.Pessoa.Email,
        Cargo = usuario.Cargo,
    };
}
