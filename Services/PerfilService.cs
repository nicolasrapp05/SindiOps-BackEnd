using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;
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

        var sindico = await _db.Sindicos.FirstOrDefaultAsync(s => s.Id == userId);
        if (sindico is not null)
        {
            sindico.Nome = nome;
            sindico.AtualizadoEm = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var response = MapSindico(sindico);
            await _supabaseAuth.SyncUserMetadataAsync(userId, response.Nome, response.Cargo);
            return response;
        }

        var funcionario = await _db.Funcionarios.FirstOrDefaultAsync(f => f.Id == userId);
        if (funcionario is not null)
        {
            funcionario.Nome = nome;
            funcionario.AtualizadoEm = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var response = MapFuncionario(funcionario);
            await _supabaseAuth.SyncUserMetadataAsync(userId, response.Nome, response.Cargo);
            return response;
        }

        throw new UnauthorizedAccessException("Usuário não encontrado");
    }

    private async Task<PerfilResponse> LoadPerfilAsync(Guid userId)
    {
        var sindico = await _db.Sindicos.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == userId);

        if (sindico is not null)
            return MapSindico(sindico);

        var funcionario = await _db.Funcionarios.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == userId);

        if (funcionario is not null)
            return MapFuncionario(funcionario);

        throw new UnauthorizedAccessException("Usuário não encontrado");
    }

    private static PerfilResponse MapSindico(Sindico sindico) => new()
    {
        Id = sindico.Id,
        Nome = sindico.Nome,
        Email = sindico.Email,
        Cargo = CargoConstants.Sindico,
    };

    private static PerfilResponse MapFuncionario(Funcionario funcionario) => new()
    {
        Id = funcionario.Id,
        Nome = funcionario.Nome,
        Email = funcionario.Email,
        Cargo = funcionario.Cargo,
    };
}
