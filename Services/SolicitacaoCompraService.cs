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

public class SolicitacaoCompraService : ISolicitacaoCompraService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;

    public SolicitacaoCompraService(SindiOpsDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<SolicitacaoCompraResponse>> GetAllAsync(
        Guid userId, SolicitacaoCompraQueryParams q)
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

        var query = _db.SolicitacoesCompra
            .Include(s => s.SolicitadoPorFuncionario)
            .Include(s => s.SolicitadoPorSindico)
            .Include(s => s.AprovadoPor)
            .Where(s => s.CondominioId == q.CondominioId);

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(s => s.Status == q.Status);

        if (!string.IsNullOrWhiteSpace(q.Categoria))
            query = query.Where(s => s.Categoria == q.Categoria);

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var items = await query
            .OrderByDescending(s => s.CriadoEm)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<SolicitacaoCompraResponse>
        {
            Data = _mapper.Map<List<SolicitacaoCompraResponse>>(items),
            TotalCount = totalCount,
            PageSize = pageSize
        };
    }

    public async Task<SolicitacaoCompraDetalheResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await _db.SolicitacoesCompra
            .Include(s => s.SolicitadoPorFuncionario)
            .Include(s => s.SolicitadoPorSindico)
            .Include(s => s.AprovadoPor)
            .Include(s => s.Cotacoes)
            .ThenInclude(c => c.Fornecedor)
            .FirstOrDefaultAsync(s => s.Id == id && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        return _mapper.Map<SolicitacaoCompraDetalheResponse>(entity);
    }

    public async Task<SolicitacaoCompraResponse> CreateAsync(CreateSolicitacaoCompraRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        if (!await UsuarioSindicoScope.IsFuncionarioOuSindicoPrincipalAsync(_db, userId, sindicoId))
            throw new ValidationException(new[]
            {
                new ValidationFailure("", "Utilizador não autorizado a criar solicitações de compra neste condomínio")
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

        var entity = new SolicitacaoCompra
        {
            CondominioId = request.CondominioId,
            SolicitadoPorFuncionarioId = solicitadoFuncionarioId,
            SolicitadoPorSindicoId = solicitadoSindicoId,
            Categoria = request.Categoria,
            Item = request.Item,
            Quantidade = request.Quantidade,
            EReposicao = request.EReposicao,
            Justificativa = request.Justificativa,
            TipoAprovacao = request.TipoAprovacao,
            Status = SolicitacaoStatus.Nova,
            CriadoEm = DateTime.UtcNow
        };

        _db.SolicitacoesCompra.Add(entity);
        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(s => s.SolicitadoPorFuncionario).LoadAsync();
        await _db.Entry(entity).Reference(s => s.SolicitadoPorSindico).LoadAsync();

        return _mapper.Map<SolicitacaoCompraResponse>(entity);
    }

    public async Task<SolicitacaoCompraResponse> UpdateAsync(Guid id, CreateSolicitacaoCompraRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await _db.SolicitacoesCompra
            .Include(s => s.SolicitadoPorFuncionario)
            .Include(s => s.SolicitadoPorSindico)
            .Include(s => s.AprovadoPor)
            .FirstOrDefaultAsync(s => s.Id == id && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        if (entity.CondominioId != request.CondominioId)
        {
            var novoOk = await _db.Condominios
                .AnyAsync(c => c.Id == request.CondominioId && c.SindicoId == sindicoId);
            if (!novoOk)
                throw new KeyNotFoundException("Condomínio não encontrado");
        }

        entity.CondominioId = request.CondominioId;
        entity.Categoria = request.Categoria;
        entity.Item = request.Item;
        entity.Quantidade = request.Quantidade;
        entity.EReposicao = request.EReposicao;
        entity.Justificativa = request.Justificativa;
        entity.TipoAprovacao = request.TipoAprovacao;
        entity.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<SolicitacaoCompraResponse>(entity);
    }

    public async Task<SolicitacaoCompraDetalheResponse> AprovarAsync(Guid id, Guid userId)
    {
        if (!await _db.Sindicos.AnyAsync(s => s.Id == userId))
            throw new UnauthorizedAccessException("Apenas o síndico pode aprovar solicitações de compra");

        var sindicoId = userId; // já é o id do síndico

        var entity = await _db.SolicitacoesCompra
            .Include(s => s.SolicitadoPorFuncionario)
            .Include(s => s.SolicitadoPorSindico)
            .Include(s => s.AprovadoPor)
            .Include(s => s.Cotacoes)
            .ThenInclude(c => c.Fornecedor)
            .FirstOrDefaultAsync(s => s.Id == id && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        entity.Status = SolicitacaoStatus.EmAndamento;
        // FK aprovado_por → funcionarios: síndico não tem linha em funcionarios
        entity.AprovadoPorId = await _db.Funcionarios.AnyAsync(f => f.Id == userId && f.SindicoId == sindicoId)
            ? userId
            : null;
        entity.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<SolicitacaoCompraDetalheResponse>(entity);
    }

    public async Task<SolicitacaoCompraResponse> UpdateStatusAsync(
        Guid id, UpdateSolicitacaoCompraStatusRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await _db.SolicitacoesCompra
            .Include(s => s.SolicitadoPorFuncionario)
            .Include(s => s.SolicitadoPorSindico)
            .Include(s => s.AprovadoPor)
            .FirstOrDefaultAsync(s => s.Id == id && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        entity.Status = request.Status;
        entity.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<SolicitacaoCompraResponse>(entity);
    }

    public async Task<List<CotacaoResponse>> GetCotacoesAsync(Guid solicitacaoId, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var pertence = await _db.SolicitacoesCompra
            .AnyAsync(s => s.Id == solicitacaoId && s.Condominio.SindicoId == sindicoId);
        if (!pertence)
            throw new KeyNotFoundException("Solicitação não encontrada");

        var cotacoes = await _db.Cotacoes
            .Include(c => c.Fornecedor)
            .Where(c => c.SolicitacaoCompraId == solicitacaoId)
            .OrderBy(c => c.CriadoEm)
            .ToListAsync();

        return _mapper.Map<List<CotacaoResponse>>(cotacoes);
    }

    public async Task<CotacaoResponse> CreateCotacaoAsync(
        Guid solicitacaoId, CreateCotacaoRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var solicitacao = await _db.SolicitacoesCompra
            .FirstOrDefaultAsync(s => s.Id == solicitacaoId && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        if (request.FornecedorId.HasValue &&
            !await _db.Fornecedores.AnyAsync(f => f.Id == request.FornecedorId && f.SindicoId == sindicoId))
            throw new KeyNotFoundException("Fornecedor não encontrado");

        var cotacao = new Cotacao
        {
            SolicitacaoCompraId = solicitacao.Id,
            FornecedorId = request.FornecedorId,
            NomeEmpresa = request.NomeEmpresa,
            NomeContato = request.NomeContato,
            NomeResponsavel = request.NomeResponsavel,
            ValorUnitario = request.ValorUnitario,
            ValorTotal = request.ValorTotal,
            FormaPagamento = request.FormaPagamento,
            DescricaoProduto = request.DescricaoProduto,
            Quantidade = request.Quantidade,
            Unidade = request.Unidade,
            Selecionada = false,
            CriadoEm = DateTime.UtcNow
        };

        _db.Cotacoes.Add(cotacao);
        await _db.SaveChangesAsync();

        await _db.Entry(cotacao).Reference(c => c.Fornecedor).LoadAsync();

        return _mapper.Map<CotacaoResponse>(cotacao);
    }

    public async Task<CotacaoResponse> UpdateCotacaoAsync(
        Guid solicitacaoId, Guid cotacaoId, CreateCotacaoRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var cotacao = await _db.Cotacoes
            .Include(c => c.Fornecedor)
            .Include(c => c.SolicitacaoCompra)
            .FirstOrDefaultAsync(c =>
                c.Id == cotacaoId &&
                c.SolicitacaoCompraId == solicitacaoId &&
                c.SolicitacaoCompra.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Cotação não encontrada");

        if (request.FornecedorId.HasValue &&
            !await _db.Fornecedores.AnyAsync(f => f.Id == request.FornecedorId && f.SindicoId == sindicoId))
            throw new KeyNotFoundException("Fornecedor não encontrado");

        cotacao.FornecedorId = request.FornecedorId;
        cotacao.NomeEmpresa = request.NomeEmpresa;
        cotacao.NomeContato = request.NomeContato;
        cotacao.NomeResponsavel = request.NomeResponsavel;
        cotacao.ValorUnitario = request.ValorUnitario;
        cotacao.ValorTotal = request.ValorTotal;
        cotacao.FormaPagamento = request.FormaPagamento;
        cotacao.DescricaoProduto = request.DescricaoProduto;
        cotacao.Quantidade = request.Quantidade;
        cotacao.Unidade = request.Unidade;

        await _db.SaveChangesAsync();

        return _mapper.Map<CotacaoResponse>(cotacao);
    }

    public async Task SelecionarCotacaoAsync(Guid solicitacaoId, Guid cotacaoId, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        await using var tx = await _db.Database.BeginTransactionAsync();

        var existeSolicitacao = await _db.SolicitacoesCompra
            .AnyAsync(s => s.Id == solicitacaoId && s.Condominio.SindicoId == sindicoId);
        if (!existeSolicitacao)
            throw new KeyNotFoundException("Solicitação não encontrada");

        var cotacoes = await _db.Cotacoes
            .Where(c => c.SolicitacaoCompraId == solicitacaoId)
            .ToListAsync();

        var alvo = cotacoes.FirstOrDefault(c => c.Id == cotacaoId)
            ?? throw new KeyNotFoundException("Cotação não encontrada");

        foreach (var c in cotacoes)
            c.Selecionada = false;

        alvo.Selecionada = true;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task DeleteCotacaoAsync(Guid solicitacaoId, Guid cotacaoId, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var cotacao = await _db.Cotacoes
            .Include(c => c.SolicitacaoCompra)
            .FirstOrDefaultAsync(c =>
                c.Id == cotacaoId &&
                c.SolicitacaoCompraId == solicitacaoId &&
                c.SolicitacaoCompra.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Cotação não encontrada");

        _db.Cotacoes.Remove(cotacao);
        await _db.SaveChangesAsync();
    }
}
