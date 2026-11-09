using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class ManutencaoObrigatoriaService : IManutencaoObrigatoriaService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;

    public ManutencaoObrigatoriaService(SindiOpsDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<ManutencaoObrigatoriaResponse>> GetAllAsync(
        Guid userId, ManutencaoObrigatoriaQueryParams q)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);
        var condominioIds = await UsuarioSindicoScope.ResolverIdsCondominioAsync(
            _db, userId, sindicoId, q.CondominioId);

        var query = _db.ManutencoesObrigatorias
            .Include(m => m.Condominio)
            .Where(m => condominioIds.Contains(m.CondominioId));

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(m =>
                (m.Observacoes != null && m.Observacoes.ToLower().Contains(term)) ||
                m.Tipo.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(m => m.Status == q.Status);

        if (!string.IsNullOrWhiteSpace(q.Tipo))
            query = query.Where(m => m.Tipo == q.Tipo);

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var sort = ListSort.Key(q.Sort, "tipo", "condominio", "vencimento", "ultimaRealizacao", "status");
        var desc = ListSort.Descending(q.Dir);
        var ordered = sort switch
        {
            "tipo" => ListSort.By(query, m => m.Tipo, desc),
            "condominio" => ListSort.By(query, m => m.Condominio.Nome, desc),
            "vencimento" => ListSort.By(query, m => m.DataVencimento, desc),
            "ultimaRealizacao" => ListSort.By(query, m => m.UltimaRealizacao, desc),
            "status" => ListSort.By(query, m => m.Status, desc),
            _ => ListSort.By(query, m => m.DataVencimento, false),
        };

        var items = await ordered
            .ThenBy(m => m.Id)
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

        await ExigirAcessoAsync(userId, sindicoId, m.CondominioId);

        return _mapper.Map<ManutencaoObrigatoriaResponse>(m);
    }

    public async Task<ManutencaoObrigatoriaResponse> CreateAsync(CreateManutencaoObrigatoriaRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        await ExigirAcessoAsync(userId, sindicoId, request.CondominioId, "Condomínio não encontrado");

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

        await ExigirAcessoAsync(userId, sindicoId, manutencao.CondominioId);
        if (manutencao.CondominioId != request.CondominioId)
            await ExigirAcessoAsync(userId, sindicoId, request.CondominioId, "Condomínio não encontrado");

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

        await ExigirAcessoAsync(userId, sindicoId, manutencao.CondominioId);

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

        await ExigirAcessoAsync(userId, sindicoId, manutencao.CondominioId);

        _db.ManutencoesObrigatorias.Remove(manutencao);
        await _db.SaveChangesAsync();
    }

    private Task ExigirAcessoAsync(
        Guid userId, Guid sindicoId, Guid condominioId, string mensagem = "Manutenção não encontrada") =>
        UsuarioSindicoScope.ExigirAcessoCondominioAsync(_db, userId, sindicoId, condominioId, mensagem);
}
