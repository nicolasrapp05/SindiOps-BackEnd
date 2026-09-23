using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class MoradorService : IMoradorService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;
    private readonly ISupabaseAuthService _supabaseAuth;

    public MoradorService(SindiOpsDbContext db, IMapper mapper, ISupabaseAuthService supabaseAuth)
    {
        _db = db;
        _mapper = mapper;
        _supabaseAuth = supabaseAuth;
    }

    public async Task<PaginatedResponse<MoradorResponse>> GetAllAsync(
        Guid condominioId, Guid sindicoId, MoradorQueryParams q)
    {
        await VerificarCondominioAsync(condominioId, sindicoId);

        var query = _db.Moradores
            .Include(m => m.Pessoa)
            .Include(m => m.Unidade)
            .ThenInclude(u => u.Bloco)
            .Where(m => m.Unidade.CondominioId == condominioId);

        if (q.BlocoId.HasValue)
            query = query.Where(m => m.Unidade.BlocoId == q.BlocoId.Value);

        if (q.UnidadeId.HasValue)
            query = query.Where(m => m.UnidadeId == q.UnidadeId.Value);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.ToLower();
            query = query.Where(m =>
                m.Pessoa.Nome.ToLower().Contains(search) ||
                m.Pessoa.Email.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var moradores = await query
            .OrderBy(m => m.Pessoa.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<MoradorResponse>
        {
            Data = _mapper.Map<List<MoradorResponse>>(moradores),
            TotalCount = totalCount,
            PageSize = pageSize
        };
    }

    public async Task<MoradorDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId)
    {
        var morador = await QueryDaCarteira(sindicoId)
            .Include(m => m.EmailLogs.OrderByDescending(e => e.EnviadoEm).Take(5))
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException("Morador não encontrado");

        return _mapper.Map<MoradorDetalheResponse>(morador);
    }

    public async Task<MoradorResponse> CreateAsync(CreateMoradorRequest request, Guid sindicoId)
    {
        await VerificarCondominioAsync(request.CondominioId, sindicoId);

        var unidade = await _db.Unidades
            .Include(u => u.Bloco)
            .FirstOrDefaultAsync(u => u.Id == request.UnidadeId && u.CondominioId == request.CondominioId)
            ?? throw new KeyNotFoundException("Unidade não encontrada neste condomínio");

        await GarantirProprietarioUnicoAsync(request.UnidadeId, request.Papel, null);

        var pessoa = await ResolverPessoaDaCarteiraAsync(request.Nome, request.Email, request.Telefone, sindicoId);

        var jaOcupa = pessoa.Id != Guid.Empty && await _db.Moradores
            .AnyAsync(m => m.UnidadeId == request.UnidadeId && m.PessoaId == pessoa.Id);
        if (jaOcupa)
            throw new ValidationException(new[]
            {
                new ValidationFailure("email", "Email já cadastrado nesta unidade")
            });

        var morador = new Morador
        {
            Pessoa = pessoa,
            UnidadeId = request.UnidadeId,
            Unidade = unidade,
            Papel = request.Papel,
            CriadoEm = DateTime.UtcNow
        };

        _db.Moradores.Add(morador);
        await _db.SaveChangesAsync();

        return _mapper.Map<MoradorResponse>(morador);
    }

    public async Task<MoradorResponse> UpdateAsync(Guid id, UpdateMoradorRequest request, Guid sindicoId)
    {
        var morador = await QueryDaCarteira(sindicoId)
            .Include(m => m.Pessoa)
            .ThenInclude(p => p.Usuario)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException("Morador não encontrado");

        var unidade = await _db.Unidades
            .Include(u => u.Bloco)
            .FirstOrDefaultAsync(u => u.Id == request.UnidadeId && u.CondominioId == morador.Unidade.CondominioId)
            ?? throw new KeyNotFoundException("Unidade não encontrada neste condomínio");

        await GarantirProprietarioUnicoAsync(request.UnidadeId, request.Papel, morador.Id);

        var email = request.Email.Trim();
        var emailNorm = email.ToLowerInvariant();
        if (morador.Pessoa.Usuario is not null &&
            !string.Equals(morador.Pessoa.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("email", "O email de um usuário com acesso não muda pelo cadastro de morador.")
            });
        }

        var outraPessoa = await BuscarPessoaDaCarteiraAsync(emailNorm, sindicoId);
        if (outraPessoa is not null && outraPessoa.Id != morador.PessoaId)
        {
            var jaOcupa = await _db.Moradores
                .AnyAsync(m => m.UnidadeId == request.UnidadeId && m.PessoaId == outraPessoa.Id && m.Id != id);
            if (jaOcupa)
                throw new ValidationException(new[]
                {
                    new ValidationFailure("email", "Email já cadastrado nesta unidade")
                });

            morador.PessoaId = outraPessoa.Id;
            morador.Pessoa = outraPessoa;
        }

        if (morador.Pessoa.Usuario is null)
        {
            morador.Pessoa.Nome = request.Nome.Trim();
            morador.Pessoa.Email = email;
            morador.Pessoa.Telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim();
        }
        else if (!string.IsNullOrWhiteSpace(request.Telefone))
        {
            morador.Pessoa.Telefone = request.Telefone.Trim();
        }

        morador.Pessoa.AtualizadoEm = DateTime.UtcNow;
        morador.UnidadeId = request.UnidadeId;
        morador.Unidade = unidade;
        morador.Papel = request.Papel;
        morador.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        if (morador.Pessoa.Usuario is not null)
            await _supabaseAuth.SyncUserMetadataAsync(morador.Pessoa.Usuario.Id, morador.Pessoa.Nome, morador.Pessoa.Usuario.Cargo);

        return _mapper.Map<MoradorResponse>(morador);
    }

    public async Task DeleteAsync(Guid id, Guid sindicoId)
    {
        var morador = await QueryDaCarteira(sindicoId)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException("Morador não encontrado");

        morador.DeletadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<PaginatedResponse<EmailLogResumoResponse>> GetEmailLogsAsync(
        Guid moradorId, Guid sindicoId, int page, int pageSize)
    {
        var pertence = await QueryDaCarteira(sindicoId).AnyAsync(m => m.Id == moradorId);

        if (!pertence)
            throw new KeyNotFoundException("Morador não encontrado");

        var query = _db.EmailLogs.Where(e => e.MoradorId == moradorId);
        var totalCount = await query.CountAsync();
        var size = Math.Clamp(pageSize, 1, 100);

        var logs = await query
            .OrderByDescending(e => e.EnviadoEm)
            .Skip((Math.Max(page, 1) - 1) * size)
            .Take(size)
            .Select(e => new EmailLogResumoResponse
            {
                Id = e.Id,
                Assunto = e.Assunto,
                EnviadoEm = e.EnviadoEm,
                StatusEntrega = e.StatusEntrega
            })
            .ToListAsync();

        return new PaginatedResponse<EmailLogResumoResponse>
        {
            Data = logs,
            TotalCount = totalCount,
            PageSize = size
        };
    }

    private IQueryable<Morador> QueryDaCarteira(Guid sindicoId) =>
        _db.Moradores
            .Include(m => m.Pessoa)
            .Include(m => m.Unidade)
            .ThenInclude(u => u.Bloco)
            .Where(m => m.Unidade.Condominio.SindicoId == sindicoId);

    private async Task VerificarCondominioAsync(Guid condominioId, Guid sindicoId)
    {
        var pertence = await _db.Condominios
            .AnyAsync(c => c.Id == condominioId && c.SindicoId == sindicoId);

        if (!pertence)
            throw new KeyNotFoundException("Condomínio não encontrado");
    }

    private async Task GarantirProprietarioUnicoAsync(Guid unidadeId, string papel, Guid? ignorarMoradorId)
    {
        if (papel != MoradorPapel.Proprietario)
            return;

        var jaTem = await _db.Moradores.AnyAsync(m =>
            m.UnidadeId == unidadeId &&
            m.Papel == MoradorPapel.Proprietario &&
            m.Id != ignorarMoradorId);

        if (jaTem)
            throw new ValidationException(new[]
            {
                new ValidationFailure("papel", "Esta unidade já tem um proprietário.")
            });
    }

    private async Task<Pessoa> ResolverPessoaDaCarteiraAsync(
        string nome, string email, string? telefone, Guid sindicoId)
    {
        var emailNorm = email.Trim().ToLowerInvariant();
        var existente = await BuscarPessoaDaCarteiraAsync(emailNorm, sindicoId);
        if (existente is not null)
        {
            if (existente.Usuario is null)
            {
                existente.Nome = nome.Trim();
                existente.Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
                existente.AtualizadoEm = DateTime.UtcNow;
            }
            else
            {
                existente.Telefone = string.IsNullOrWhiteSpace(telefone) ? existente.Telefone : telefone.Trim();
                existente.AtualizadoEm = DateTime.UtcNow;
            }

            return existente;
        }

        var pessoa = new Pessoa
        {
            Nome = nome.Trim(),
            Email = email.Trim(),
            Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim(),
            CriadoEm = DateTime.UtcNow
        };
        _db.Pessoas.Add(pessoa);
        return pessoa;
    }

    private Task<Pessoa?> BuscarPessoaDaCarteiraAsync(string emailNorm, Guid sindicoId) =>
        _db.Pessoas
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p =>
                p.Email.ToLower() == emailNorm &&
                (p.Moradores.Any(m => m.Unidade.Condominio.SindicoId == sindicoId)
                 || (p.Usuario != null && (p.Usuario.Id == sindicoId || p.Usuario.SindicoId == sindicoId))));
}
