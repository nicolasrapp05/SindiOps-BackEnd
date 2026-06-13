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

public class ContratoService : IContratoService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;

    public ContratoService(SindiOpsDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<ContratoResponse>> GetAllAsync(Guid sindicoId, ContratoQueryParams q)
    {
        var query = _db.Contratos
            .Include(c => c.Fornecedor)
            .Where(c => c.Condominio.SindicoId == sindicoId);

        if (q.CondominioId.HasValue)
            query = query.Where(c => c.CondominioId == q.CondominioId.Value);

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

        var contratos = await query
            .OrderByDescending(c => c.CriadoEm)
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

    public async Task<ContratoDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId)
    {
        var contrato = await _db.Contratos
            .Include(c => c.Fornecedor)
            .FirstOrDefaultAsync(c => c.Id == id && c.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Contrato não encontrado");

        return _mapper.Map<ContratoDetalheResponse>(contrato);
    }

    public async Task<ContratoDetalheResponse> CreateAsync(CreateContratoRequest request, Guid sindicoId)
    {
        await VerificarCondominioAsync(request.CondominioId, sindicoId);
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
            Status = ContratoStatus.Active,
            CriadoEm = DateTime.UtcNow
        };

        _db.Contratos.Add(contrato);
        await _db.SaveChangesAsync();

        await _db.Entry(contrato).Reference(c => c.Fornecedor).LoadAsync();

        return _mapper.Map<ContratoDetalheResponse>(contrato);
    }

    public async Task<ContratoDetalheResponse> UpdateAsync(Guid id, CreateContratoRequest request, Guid sindicoId)
    {
        var contrato = await _db.Contratos
            .Include(c => c.Fornecedor)
            .FirstOrDefaultAsync(c => c.Id == id && c.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Contrato não encontrado");

        // permite trocar condomínio/fornecedor — revalida propriedade
        if (contrato.CondominioId != request.CondominioId)
            await VerificarCondominioAsync(request.CondominioId, sindicoId);

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

        await _db.SaveChangesAsync();

        // recarrega fornecedor caso tenha mudado
        await _db.Entry(contrato).Reference(c => c.Fornecedor).LoadAsync();

        return _mapper.Map<ContratoDetalheResponse>(contrato);
    }

    public async Task<ContratoDetalheResponse> UpdateStatusAsync(
        Guid id, UpdateContratoStatusRequest request, Guid sindicoId)
    {
        var contrato = await _db.Contratos
            .Include(c => c.Fornecedor)
            .FirstOrDefaultAsync(c => c.Id == id && c.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Contrato não encontrado");

        contrato.Status = request.Status;
        contrato.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<ContratoDetalheResponse>(contrato);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task VerificarCondominioAsync(Guid condominioId, Guid sindicoId)
    {
        var pertence = await _db.Condominios
            .AnyAsync(c => c.Id == condominioId && c.SindicoId == sindicoId);

        if (!pertence)
            throw new KeyNotFoundException("Condomínio não encontrado");
    }

    private async Task VerificarFornecedorAsync(Guid fornecedorId, Guid sindicoId)
    {
        var pertence = await _db.Fornecedores
            .AnyAsync(f => f.Id == fornecedorId && f.SindicoId == sindicoId);

        if (!pertence)
            throw new KeyNotFoundException("Fornecedor não encontrado");
    }
}
