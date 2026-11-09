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

public class ContratoService : IContratoService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;

    public ContratoService(SindiOpsDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<ContratoResponse>> GetAllAsync(
        Guid sindicoId, ContratoQueryParams q, Guid userId = default)
    {
        var usuarioId = await UsuarioAsync(sindicoId, userId);
        var condominioIds = await UsuarioSindicoScope.ResolverIdsCondominioAsync(
            _db, usuarioId, sindicoId, q.CondominioId);
        await SincronizarStatusAutomaticosAsync(sindicoId, condominioIds);

        var query = _db.Contratos
            .Include(c => c.Condominio)
            .Include(c => c.Fornecedor)
            .Where(c => c.Condominio.SindicoId == sindicoId && condominioIds.Contains(c.CondominioId));

        if (q.FornecedorId.HasValue)
            query = query.Where(c => c.FornecedorId == q.FornecedorId.Value);

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(c => c.Status == q.Status);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(c =>
                c.Fornecedor.Nome.ToLower().Contains(term) ||
                c.TipoServico.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var sort = ListSort.Key(q.Sort, "condominio", "tipo", "fornecedor", "inicio", "termino", "valor", "status");
        var desc = ListSort.Descending(q.Dir);
        var ordered = sort switch
        {
            "condominio" => ListSort.By(query, c => c.Condominio.Nome, desc),
            "tipo" => ListSort.By(query, c => c.TipoServico, desc),
            "fornecedor" => ListSort.By(query, c => c.Fornecedor.Nome, desc),
            "inicio" => ListSort.By(query, c => c.DataInicio, desc),
            "termino" => ListSort.By(query, c => c.DataFim, desc),
            "valor" => ListSort.By(query, c => c.ValorMensal, desc),
            "status" => ListSort.By(query, c => c.Status, desc),
            _ => ListSort.By(query, c => c.CriadoEm, true),
        };

        var contratos = await ordered
            .ThenBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<ContratoResponse>
        {
            Data = _mapper.Map<List<ContratoResponse>>(contratos),
            TotalCount = totalCount,
            PageSize = pageSize
        };
    }

    public async Task<ContratoDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId, Guid userId = default)
    {
        var contrato = await _db.Contratos
            .Include(c => c.Condominio)
            .Include(c => c.Fornecedor)
            .FirstOrDefaultAsync(c => c.Id == id && c.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Contrato não encontrado");

        await ExigirAcessoAsync(await UsuarioAsync(sindicoId, userId), sindicoId, contrato.CondominioId);

        if (ContratoStatusCalculator.TryApplyAutomaticStatus(contrato))
            await _db.SaveChangesAsync();

        return _mapper.Map<ContratoDetalheResponse>(contrato);
    }

    public async Task<ContratoDetalheResponse> CreateAsync(
        CreateContratoRequest request, Guid sindicoId, Guid userId = default)
    {
        await ExigirAcessoAsync(
            await UsuarioAsync(sindicoId, userId), sindicoId, request.CondominioId, "Condomínio não encontrado");
        await VerificarFornecedorAsync(request.FornecedorId, sindicoId);

        var contrato = new Contrato
        {
            CondominioId = request.CondominioId,
            FornecedorId = request.FornecedorId,
            TipoServico = request.TipoServico,
            NomeContato = request.NomeContato,
            TelefoneContato = request.TelefoneContato,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            ValorMensal = request.ValorMensal,
            IndiceReajuste = request.IndiceReajuste,
            CondicoesRenovacao = request.CondicoesRenovacao,
            CondicoesRescisao = request.CondicoesRescisao,
            CriadoEm = DateTime.UtcNow
        };

        ContratoStatusCalculator.TryApplyAutomaticStatus(contrato);

        _db.Contratos.Add(contrato);
        await _db.SaveChangesAsync();

        await _db.Entry(contrato).Reference(c => c.Condominio).LoadAsync();
        await _db.Entry(contrato).Reference(c => c.Fornecedor).LoadAsync();

        return _mapper.Map<ContratoDetalheResponse>(contrato);
    }

    public async Task<ContratoDetalheResponse> UpdateAsync(
        Guid id, CreateContratoRequest request, Guid sindicoId, Guid userId = default)
    {
        var usuarioId = await UsuarioAsync(sindicoId, userId);
        var contrato = await _db.Contratos
            .Include(c => c.Condominio)
            .Include(c => c.Fornecedor)
            .FirstOrDefaultAsync(c => c.Id == id && c.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Contrato não encontrado");

        await ExigirAcessoAsync(usuarioId, sindicoId, contrato.CondominioId);
        if (contrato.CondominioId != request.CondominioId)
            await ExigirAcessoAsync(usuarioId, sindicoId, request.CondominioId, "Condomínio não encontrado");

        if (contrato.FornecedorId != request.FornecedorId)
            await VerificarFornecedorAsync(request.FornecedorId, sindicoId);

        contrato.CondominioId = request.CondominioId;
        contrato.FornecedorId = request.FornecedorId;
        contrato.TipoServico = request.TipoServico;
        contrato.NomeContato = request.NomeContato;
        contrato.TelefoneContato = request.TelefoneContato;
        contrato.DataInicio = request.DataInicio;
        contrato.DataFim = request.DataFim;
        contrato.ValorMensal = request.ValorMensal;
        contrato.IndiceReajuste = request.IndiceReajuste;
        contrato.CondicoesRenovacao = request.CondicoesRenovacao;
        contrato.CondicoesRescisao = request.CondicoesRescisao;
        contrato.AtualizadoEm = DateTime.UtcNow;

        ContratoStatusCalculator.TryApplyAutomaticStatus(contrato);

        await _db.SaveChangesAsync();

        if (contrato.Condominio.Id != contrato.CondominioId)
            contrato.Condominio = await _db.Condominios.FirstAsync(c => c.Id == contrato.CondominioId);
        await _db.Entry(contrato).Reference(c => c.Fornecedor).LoadAsync();

        return _mapper.Map<ContratoDetalheResponse>(contrato);
    }

    public async Task<ContratoDetalheResponse> UpdateStatusAsync(
        Guid id, UpdateContratoStatusRequest request, Guid sindicoId, Guid userId = default)
    {
        var contrato = await _db.Contratos
            .Include(c => c.Condominio)
            .Include(c => c.Fornecedor)
            .FirstOrDefaultAsync(c => c.Id == id && c.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Contrato não encontrado");

        await ExigirAcessoAsync(await UsuarioAsync(sindicoId, userId), sindicoId, contrato.CondominioId);

        if (request.Status != ContratoStatus.Cancelled)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("status",
                    "Alterações entre vigente, expirando e expirado são automáticas. Atualize a data de término ou reative o contrato.")
            });
        }

        if (contrato.Status == ContratoStatus.Cancelled)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("status", "Este contrato já está cancelado.")
            });
        }

        contrato.Status = ContratoStatus.Cancelled;
        contrato.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<ContratoDetalheResponse>(contrato);
    }

    public async Task<ContratoDetalheResponse> ReativarAsync(Guid id, Guid sindicoId, Guid userId = default)
    {
        var contrato = await _db.Contratos
            .Include(c => c.Condominio)
            .Include(c => c.Fornecedor)
            .FirstOrDefaultAsync(c => c.Id == id && c.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Contrato não encontrado");

        await ExigirAcessoAsync(await UsuarioAsync(sindicoId, userId), sindicoId, contrato.CondominioId);

        if (contrato.Status != ContratoStatus.Cancelled)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("status", "Apenas contratos cancelados podem ser reativados.")
            });
        }

        ContratoStatusCalculator.ApplyReativacao(contrato);
        await _db.SaveChangesAsync();

        return _mapper.Map<ContratoDetalheResponse>(contrato);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task SincronizarStatusAutomaticosAsync(Guid sindicoId, IReadOnlyCollection<Guid> condominioIds)
    {
        if (condominioIds.Count == 0)
            return;

        var query = _db.Contratos
            .Where(c => c.Condominio.SindicoId == sindicoId
                        && c.Status != ContratoStatus.Cancelled
                        && condominioIds.Contains(c.CondominioId));

        var contratos = await query.ToListAsync();
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var changed = false;

        foreach (var contrato in contratos)
        {
            if (ContratoStatusCalculator.TryApplyAutomaticStatus(contrato, hoje))
                changed = true;
        }

        if (changed)
            await _db.SaveChangesAsync();
    }

    private async Task<Guid> UsuarioAsync(Guid sindicoId, Guid userId)
    {
        if (userId != Guid.Empty)
            return userId;

        return await _db.Sindicos.AsNoTracking()
            .Where(s => s.Id == sindicoId)
            .Select(s => s.UsuarioId)
            .FirstAsync();
    }

    private Task ExigirAcessoAsync(
        Guid userId, Guid sindicoId, Guid condominioId, string mensagem = "Contrato não encontrado") =>
        UsuarioSindicoScope.ExigirAcessoCondominioAsync(_db, userId, sindicoId, condominioId, mensagem);

    private async Task VerificarFornecedorAsync(Guid fornecedorId, Guid sindicoId)
    {
        var pertence = await _db.Fornecedores
            .AnyAsync(f => f.Id == fornecedorId && f.SindicoId == sindicoId);

        if (!pertence)
            throw new KeyNotFoundException("Fornecedor não encontrado");
    }
}
