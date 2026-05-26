using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Entities;
using SindiCore.API.Helpers;
using SindiCore.API.Infrastructure.Data;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Services;

public class MoradorService : IMoradorService
{
    private readonly SindiCoreDbContext _db;
    private readonly IMapper _mapper;

    public MoradorService(SindiCoreDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<MoradorResponse>> GetAllAsync(
        Guid condominioId, Guid sindicoId, MoradorQueryParams q)
    {
        await VerificarCondominioAsync(condominioId, sindicoId);

        var query = _db.Moradores
            .Include(m => m.Bloco)
            .Include(m => m.Unidade)
            .Where(m => m.CondominioId == condominioId);

        if (q.BlocoId.HasValue)
            query = query.Where(m => m.BlocoId == q.BlocoId.Value);

        if (q.UnidadeId.HasValue)
            query = query.Where(m => m.UnidadeId == q.UnidadeId.Value);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.ToLower();
            query = query.Where(m =>
                m.Nome.ToLower().Contains(search) ||
                m.Email.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var moradores = await query
            .OrderBy(m => m.Nome)
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
        var morador = await _db.Moradores
            .Include(m => m.Bloco)
            .Include(m => m.Unidade)
            .Include(m => m.EmailLogs.OrderByDescending(e => e.EnviadoEm).Take(5))
            .FirstOrDefaultAsync(m => m.Id == id && m.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Morador não encontrado");

        return _mapper.Map<MoradorDetalheResponse>(morador);
    }

    public async Task<MoradorResponse> CreateAsync(CreateMoradorRequest request, Guid sindicoId)
    {
        await VerificarCondominioAsync(request.CondominioId, sindicoId);

        var unidade = await _db.Unidades
            .FirstOrDefaultAsync(u => u.Id == request.UnidadeId && u.CondominioId == request.CondominioId)
            ?? throw new KeyNotFoundException("Unidade não encontrada neste condomínio");

        var emailDuplicado = await _db.Moradores
            .AnyAsync(m => m.UnidadeId == request.UnidadeId && m.Email == request.Email);

        if (emailDuplicado)
            throw new ValidationException(new[]
            {
                new ValidationFailure("email", "Email já cadastrado nesta unidade")
            });

        var morador = new Morador
        {
            CondominioId = request.CondominioId,
            BlocoId = unidade.BlocoId,
            UnidadeId = request.UnidadeId,
            Nome = request.Nome,
            Email = request.Email,
            Telefone = request.Telefone,
            CriadoEm = DateTime.UtcNow
        };

        _db.Moradores.Add(morador);
        await _db.SaveChangesAsync();

        await _db.Entry(morador).Reference(m => m.Bloco).LoadAsync();
        await _db.Entry(morador).Reference(m => m.Unidade).LoadAsync();

        return _mapper.Map<MoradorResponse>(morador);
    }

    public async Task<MoradorResponse> UpdateAsync(Guid id, UpdateMoradorRequest request, Guid sindicoId)
    {
        var morador = await _db.Moradores
            .FirstOrDefaultAsync(m => m.Id == id && m.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Morador não encontrado");

        var unidade = await _db.Unidades
            .FirstOrDefaultAsync(u => u.Id == request.UnidadeId && u.CondominioId == morador.CondominioId)
            ?? throw new KeyNotFoundException("Unidade não encontrada neste condomínio");

        if (morador.Email != request.Email)
        {
            var emailDuplicado = await _db.Moradores
                .AnyAsync(m => m.UnidadeId == request.UnidadeId && m.Email == request.Email && m.Id != id);

            if (emailDuplicado)
                throw new ValidationException(new[]
                {
                    new ValidationFailure("email", "Email já cadastrado nesta unidade")
                });
        }

        morador.Nome = request.Nome;
        morador.Email = request.Email;
        morador.Telefone = request.Telefone;
        morador.UnidadeId = request.UnidadeId;
        morador.BlocoId = unidade.BlocoId;
        morador.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _db.Entry(morador).Reference(m => m.Bloco).LoadAsync();
        await _db.Entry(morador).Reference(m => m.Unidade).LoadAsync();

        return _mapper.Map<MoradorResponse>(morador);
    }

    public async Task DeleteAsync(Guid id, Guid sindicoId)
    {
        var morador = await _db.Moradores
            .FirstOrDefaultAsync(m => m.Id == id && m.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Morador não encontrado");

        // soft delete — Global Query Filter oculta automaticamente nas queries futuras
        morador.DeletadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<PaginatedResponse<EmailLogResumoResponse>> GetEmailLogsAsync(
        Guid moradorId, Guid sindicoId, int page, int pageSize)
    {
        var pertence = await _db.Moradores
            .AnyAsync(m => m.Id == moradorId && m.Condominio.SindicoId == sindicoId);

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

    // ── helpers ─────────────────────────────────────────────────────────────

    private async Task VerificarCondominioAsync(Guid condominioId, Guid sindicoId)
    {
        var pertence = await _db.Condominios
            .AnyAsync(c => c.Id == condominioId && c.SindicoId == sindicoId);

        if (!pertence)
            throw new KeyNotFoundException("Condomínio não encontrado");
    }
}
