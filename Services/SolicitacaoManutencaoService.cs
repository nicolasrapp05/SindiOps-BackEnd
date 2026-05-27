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

public class SolicitacaoManutencaoService : ISolicitacaoManutencaoService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;

    public SolicitacaoManutencaoService(SindiOpsDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<SolicitacaoManutencaoResponse>> GetAllAsync(
        Guid userId, SolicitacaoManutencaoQueryParams q)
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

        var query = _db.SolicitacoesManutencao
            .Include(s => s.Fornecedor)
            .Include(s => s.SolicitadoPorFuncionario)
            .Include(s => s.SolicitadoPorSindico)
            .Where(s => s.CondominioId == q.CondominioId);

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(s => s.Status == q.Status);

        if (!string.IsNullOrWhiteSpace(q.TipoServico))
            query = query.Where(s => s.Tipo == q.TipoServico);

        if (!string.IsNullOrWhiteSpace(q.Responsavel))
            query = query.Where(s => s.Responsavel == q.Responsavel);

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var items = await query
            .OrderByDescending(s => s.CriadoEm)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<SolicitacaoManutencaoResponse>
        {
            Data = _mapper.Map<List<SolicitacaoManutencaoResponse>>(items),
            TotalCount = totalCount,
            PageSize = pageSize
        };
    }

    public async Task<SolicitacaoManutencaoResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var s = await _db.SolicitacoesManutencao
            .Include(x => x.Fornecedor)
            .Include(x => x.SolicitadoPorFuncionario)
            .Include(x => x.SolicitadoPorSindico)
            .FirstOrDefaultAsync(x => x.Id == id && x.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        return _mapper.Map<SolicitacaoManutencaoResponse>(s);
    }

    public async Task<SolicitacaoManutencaoResponse> CreateAsync(CreateSolicitacaoManutencaoRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        if (!await UsuarioSindicoScope.IsFuncionarioOuSindicoPrincipalAsync(_db, userId, sindicoId))
            throw new ValidationException(new[]
            {
                new ValidationFailure("", "Utilizador não autorizado a registrar solicitações de manutenção neste condomínio")
            });

        Guid? solicitadoFuncionarioId = null;
        Guid? solicitadoSindicoId = null;
        if (await UsuarioSindicoScope.IsFuncionarioDoSindicoAsync(_db, userId, sindicoId))
            solicitadoFuncionarioId = userId;
        else
            solicitadoSindicoId = sindicoId;

        var condominioOk = await _db.Condominios
            .AnyAsync(c => c.Id == request.CondominioId && c.SindicoId == sindicoId);
        if (!condominioOk)
            throw new KeyNotFoundException("Condomínio não encontrado");

        Guid? fornecedorId = request.FornecedorId;
        if (request.Responsavel == ResponsavelSolicitacao.Zelador)
            fornecedorId = null;
        else if (fornecedorId.HasValue &&
                 !await _db.Fornecedores.AnyAsync(f => f.Id == fornecedorId && f.SindicoId == sindicoId))
            throw new KeyNotFoundException("Fornecedor não encontrado");

        var entity = new SolicitacaoManutencao
        {
            CondominioId = request.CondominioId,
            SolicitadoPorFuncionarioId = solicitadoFuncionarioId,
            SolicitadoPorSindicoId = solicitadoSindicoId,
            FornecedorId = fornecedorId,
            Local = request.Local,
            Tipo = request.TipoServico,
            Responsavel = request.Responsavel,
            Descricao = request.Descricao,
            Status = SolicitacaoStatus.Nova,
            CriadoEm = DateTime.UtcNow
        };

        _db.SolicitacoesManutencao.Add(entity);
        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(x => x.Fornecedor).LoadAsync();
        await _db.Entry(entity).Reference(x => x.SolicitadoPorFuncionario).LoadAsync();
        await _db.Entry(entity).Reference(x => x.SolicitadoPorSindico).LoadAsync();

        return _mapper.Map<SolicitacaoManutencaoResponse>(entity);
    }

    public async Task<SolicitacaoManutencaoResponse> UpdateAsync(Guid id, CreateSolicitacaoManutencaoRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await _db.SolicitacoesManutencao
            .Include(s => s.Fornecedor)
            .Include(s => s.SolicitadoPorFuncionario)
            .Include(s => s.SolicitadoPorSindico)
            .FirstOrDefaultAsync(s => s.Id == id && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        if (entity.CondominioId != request.CondominioId)
        {
            var novoOk = await _db.Condominios
                .AnyAsync(c => c.Id == request.CondominioId && c.SindicoId == sindicoId);
            if (!novoOk)
                throw new KeyNotFoundException("Condomínio não encontrado");
        }

        Guid? fornecedorId = request.FornecedorId;
        if (request.Responsavel == ResponsavelSolicitacao.Zelador)
            fornecedorId = null;
        else if (fornecedorId.HasValue &&
                 !await _db.Fornecedores.AnyAsync(f => f.Id == fornecedorId && f.SindicoId == sindicoId))
            throw new KeyNotFoundException("Fornecedor não encontrado");

        entity.CondominioId = request.CondominioId;
        entity.FornecedorId = fornecedorId;
        entity.Local = request.Local;
        entity.Tipo = request.TipoServico;
        entity.Responsavel = request.Responsavel;
        entity.Descricao = request.Descricao;
        entity.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<SolicitacaoManutencaoResponse>(entity);
    }

    public async Task<SolicitacaoManutencaoResponse> UpdateStatusAsync(
        Guid id, UpdateSolicitacaoStatusRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await _db.SolicitacoesManutencao
            .Include(s => s.Fornecedor)
            .Include(s => s.SolicitadoPorFuncionario)
            .Include(s => s.SolicitadoPorSindico)
            .FirstOrDefaultAsync(s => s.Id == id && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        entity.Status = request.Status;
        if (request.Status == SolicitacaoStatus.Finalizada)
            entity.DataConclusao = request.DataConclusao;
        else
            entity.DataConclusao = null;

        entity.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<SolicitacaoManutencaoResponse>(entity);
    }
}
