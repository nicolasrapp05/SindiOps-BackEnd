using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using SindiCore.API.Constants;
using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Entities;
using SindiCore.API.Helpers;
using SindiCore.API.Infrastructure.Data;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Services;

public class ManutencaoObrigatoriaService : IManutencaoObrigatoriaService
{
    private readonly SindiCoreDbContext _db;
    private readonly IMapper _mapper;

    public ManutencaoObrigatoriaService(SindiCoreDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<ManutencaoObrigatoriaResponse>> GetAllAsync(
        Guid userId, ManutencaoObrigatoriaQueryParams q)
    {
        if (q.CondominioId == Guid.Empty)
            throw new ValidationException(new[]
            {
                new ValidationFailure("condominioId", "condominioId é obrigatório")
            });

        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var condominioOk = await _db.Condominios
            .AnyAsync(c => c.Id == q.CondominioId && c.SindicoId == sindicoId);
        if (!condominioOk)
            throw new KeyNotFoundException("Condomínio não encontrado");

        var query = _db.ManutencoesObrigatorias
            .Include(m => m.Condominio)
            .Where(m => m.CondominioId == q.CondominioId);

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(m => m.Status == q.Status);

        if (!string.IsNullOrWhiteSpace(q.Tipo))
            query = query.Where(m => m.Tipo == q.Tipo);

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var items = await query
            .OrderBy(m => m.DataVencimento)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<ManutencaoObrigatoriaResponse>
        {
            Data = _mapper.Map<List<ManutencaoObrigatoriaResponse>>(items),
            TotalCount = totalCount,
            PageSize = pageSize
        };
    }

    public async Task<ManutencaoObrigatoriaResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var m = await _db.ManutencoesObrigatorias
            .Include(x => x.Condominio)
            .FirstOrDefaultAsync(x => x.Id == id && x.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Manutenção não encontrada");

        return _mapper.Map<ManutencaoObrigatoriaResponse>(m);
    }

    public async Task<ManutencaoObrigatoriaResponse> CreateAsync(CreateManutencaoObrigatoriaRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var condominioOk = await _db.Condominios
            .AnyAsync(c => c.Id == request.CondominioId && c.SindicoId == sindicoId);
        if (!condominioOk)
            throw new KeyNotFoundException("Condomínio não encontrado");

        var manutencao = new ManutencaoObrigatoria
        {
            CondominioId = request.CondominioId,
            Tipo = request.Tipo,
            DataVencimento = request.DataVencimento,
            UltimaRealizacao = request.UltimaRealizacao,
            Observacoes = request.Observacoes,
            Status = ManutencaoStatusHelper.CalcularStatus(request.DataVencimento),
            CriadoEm = DateTime.UtcNow
        };

        _db.ManutencoesObrigatorias.Add(manutencao);
        await _db.SaveChangesAsync();

        await _db.Entry(manutencao).Reference(x => x.Condominio).LoadAsync();

        return _mapper.Map<ManutencaoObrigatoriaResponse>(manutencao);
    }

    public async Task<ManutencaoObrigatoriaResponse> UpdateAsync(Guid id, CreateManutencaoObrigatoriaRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var manutencao = await _db.ManutencoesObrigatorias
            .Include(m => m.Condominio)
            .FirstOrDefaultAsync(m => m.Id == id && m.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Manutenção não encontrada");

        if (manutencao.CondominioId != request.CondominioId)
        {
            var novoOk = await _db.Condominios
                .AnyAsync(c => c.Id == request.CondominioId && c.SindicoId == sindicoId);
            if (!novoOk)
                throw new KeyNotFoundException("Condomínio não encontrado");
        }

        manutencao.CondominioId = request.CondominioId;
        manutencao.Tipo = request.Tipo;
        manutencao.DataVencimento = request.DataVencimento;
        manutencao.UltimaRealizacao = request.UltimaRealizacao;
        manutencao.Observacoes = request.Observacoes;
        manutencao.Status = ManutencaoStatusHelper.CalcularStatus(request.DataVencimento);
        manutencao.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<ManutencaoObrigatoriaResponse>(manutencao);
    }

    public async Task<ManutencaoObrigatoriaResponse> RealizarAsync(Guid id, RealizarManutencaoRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var manutencao = await _db.ManutencoesObrigatorias
            .Include(m => m.Condominio)
            .FirstOrDefaultAsync(m => m.Id == id && m.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Manutenção não encontrada");

        manutencao.UltimaRealizacao = request.DataRealizacao;

        if (request.Observacoes != null)
            manutencao.Observacoes = request.Observacoes;

        var meses = ManutencaoObrigatoriaTipo.GetMesesAposRealizacao(manutencao.Tipo);
        manutencao.DataVencimento = request.DataRealizacao.AddMonths(meses);
        manutencao.Status = ManutencaoStatusHelper.CalcularStatus(manutencao.DataVencimento);
        manutencao.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<ManutencaoObrigatoriaResponse>(manutencao);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var manutencao = await _db.ManutencoesObrigatorias
            .Include(m => m.Condominio)
            .FirstOrDefaultAsync(m => m.Id == id && m.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Manutenção não encontrada");

        _db.ManutencoesObrigatorias.Remove(manutencao);
        await _db.SaveChangesAsync();
    }
}
