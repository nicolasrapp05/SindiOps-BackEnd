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
            .Where(s => s.CondominioId == q.CondominioId);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(s =>
                (s.Local != null && s.Local.ToLower().Contains(term)) ||
                (s.Descricao != null && s.Descricao.ToLower().Contains(term)) ||
                (s.Fornecedor != null && s.Fornecedor.Nome.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(s => s.Status == q.Status);

        if (!string.IsNullOrWhiteSpace(q.TipoServico))
            query = query.Where(s => s.TipoManutencao == q.TipoServico);

        if (!string.IsNullOrWhiteSpace(q.Responsavel))
            query = query.Where(s => s.Responsavel == q.Responsavel);

        if (!string.IsNullOrWhiteSpace(q.SituacaoOrcamento))
            query = FiltrarSituacao(query, q.SituacaoOrcamento);

        if (q.PendenteOrcamento == true)
        {
            query = query.Where(s =>
                (s.Status == SolicitacaoStatus.Nova || s.Status == SolicitacaoStatus.EmAndamento) &&
                s.OrcamentoAprovadoPorId == null);
        }

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var items = await ComReferencias(query)
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

        var s = await ComReferencias(_db.SolicitacoesManutencao)
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
            SolicitadoPorId = userId,
            Tipo = SolicitacaoTipo.Manutencao,
            FornecedorId = fornecedorId,
            Local = request.Local,
            TipoManutencao = request.TipoServico,
            Responsavel = request.Responsavel,
            Descricao = request.Descricao,
            Status = SolicitacaoStatus.Nova,
            CriadoEm = DateTime.UtcNow
        };

        _db.SolicitacoesManutencao.Add(entity);
        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(x => x.Fornecedor).LoadAsync();
        await _db.Entry(entity).Reference(x => x.SolicitadoPor).LoadAsync();
        await UsuarioPapeis.CarregarAsync(_db, entity.SolicitadoPor);

        return _mapper.Map<SolicitacaoManutencaoResponse>(entity);
    }

    public async Task<SolicitacaoManutencaoResponse> UpdateAsync(Guid id, CreateSolicitacaoManutencaoRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await ComReferencias(_db.SolicitacoesManutencao)
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

        if (entity.OrcamentoAprovadoPorId != null &&
            (entity.FornecedorId != fornecedorId || entity.Responsavel != request.Responsavel))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    "fornecedorId",
                    "Não é possível alterar o responsável ou o fornecedor enquanto o orçamento estiver aprovado.")
            });
        }

        entity.CondominioId = request.CondominioId;
        entity.FornecedorId = fornecedorId;
        entity.Local = request.Local;
        entity.TipoManutencao = request.TipoServico;
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

        var entity = await ComReferencias(_db.SolicitacoesManutencao)
            .FirstOrDefaultAsync(s => s.Id == id && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        if (!TransicaoStatusPermitida(entity.Status, request.Status))
            throw new ValidationException(new[]
            {
                new ValidationFailure("status", $"Transição de status inválida: {entity.Status} → {request.Status}")
            });

        entity.Status = request.Status;
        if (request.Status == SolicitacaoStatus.Finalizada)
            entity.DataConclusao = request.DataConclusao;
        else
            entity.DataConclusao = null;

        entity.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<SolicitacaoManutencaoResponse>(entity);
    }

    private static IQueryable<SolicitacaoManutencao> ComReferencias(IQueryable<SolicitacaoManutencao> query) =>
        query
            .Include(s => s.Fornecedor)
            .Include(s => s.Orcamentos)
            .Include(s => s.OrcamentoAprovadoPor!).ThenInclude(u => u.Sindico)
            .Include(s => s.OrcamentoAprovadoPor!).ThenInclude(u => u.Funcionario)
            .Include(s => s.SolicitadoPor).ThenInclude(u => u.Sindico)
            .Include(s => s.SolicitadoPor).ThenInclude(u => u.Funcionario);

    private static IQueryable<SolicitacaoManutencao> FiltrarSituacao(
        IQueryable<SolicitacaoManutencao> query, string situacao) =>
        situacao switch
        {
            SituacaoOrcamento.Encerrado => query.Where(s =>
                (s.Status == SolicitacaoStatus.Finalizada || s.Status == SolicitacaoStatus.Cancelada) &&
                s.Orcamentos.Any()),
            SituacaoOrcamento.Aprovado => query.Where(s =>
                s.OrcamentoAprovadoPorId != null &&
                s.Status != SolicitacaoStatus.Finalizada &&
                s.Status != SolicitacaoStatus.Cancelada),
            SituacaoOrcamento.PropostaSelecionada => query.Where(s =>
                s.OrcamentoAprovadoPorId == null &&
                s.Status != SolicitacaoStatus.Finalizada &&
                s.Status != SolicitacaoStatus.Cancelada &&
                s.Orcamentos.Any(o => o.Selecionada)),
            SituacaoOrcamento.AguardandoProposta => query.Where(s =>
                s.OrcamentoAprovadoPorId == null &&
                s.Status != SolicitacaoStatus.Finalizada &&
                s.Status != SolicitacaoStatus.Cancelada &&
                s.Orcamentos.Any() &&
                !s.Orcamentos.Any(o => o.Selecionada)),
            SituacaoOrcamento.SemOrcamento => query.Where(s => !s.Orcamentos.Any()),
            _ => query,
        };

    private static bool TransicaoStatusPermitida(string atual, string novo)
    {
        if (atual == novo)
            return true;

        return (atual, novo) switch
        {
            (SolicitacaoStatus.Nova, SolicitacaoStatus.EmAndamento) => true,
            (SolicitacaoStatus.Nova, SolicitacaoStatus.Cancelada) => true,
            (SolicitacaoStatus.Cancelada, SolicitacaoStatus.EmAndamento) => true,
            (SolicitacaoStatus.EmAndamento, SolicitacaoStatus.Finalizada) => true,
            (SolicitacaoStatus.EmAndamento, SolicitacaoStatus.Cancelada) => true,
            _ => false
        };
    }
}
