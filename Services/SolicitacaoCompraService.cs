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

        var query = ComReferencias(_db.SolicitacoesCompra)
            .Where(s => s.CondominioId == q.CondominioId);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(s =>
                s.Itens.Any(i => i.Descricao.ToLower().Contains(term)) ||
                (s.Justificativa != null && s.Justificativa.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(s => s.Status == q.Status);

        if (!string.IsNullOrWhiteSpace(q.Categoria))
            query = query.Where(s => s.Itens.Any(i => i.Categoria == q.Categoria));

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

        var entity = await ComReferencias(_db.SolicitacoesCompra)
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

        var condominioOk = await _db.Condominios
            .AnyAsync(c => c.Id == request.CondominioId && c.SindicoId == sindicoId);
        if (!condominioOk)
            throw new KeyNotFoundException("Condomínio não encontrado");

        var entity = new SolicitacaoCompra
        {
            CondominioId = request.CondominioId,
            SolicitadoPorId = userId,
            Tipo = SolicitacaoTipo.Compra,
            Justificativa = request.Justificativa,
            TipoAprovacao = request.TipoAprovacao,
            Status = SolicitacaoStatus.Nova,
            CriadoEm = DateTime.UtcNow,
            Itens = request.Itens.Select((pedido, index) => CriarItem(pedido, index)).ToList()
        };

        _db.SolicitacoesCompra.Add(entity);
        await _db.SaveChangesAsync();

        await CarregarAutorAsync(entity);

        return _mapper.Map<SolicitacaoCompraResponse>(entity);
    }

    public async Task<SolicitacaoCompraResponse> UpdateAsync(Guid id, CreateSolicitacaoCompraRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await ComReferencias(_db.SolicitacoesCompra)
            .FirstOrDefaultAsync(s => s.Id == id && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        EnsureStatus(entity, SolicitacaoStatus.Nova, "editar a solicitação");

        if (entity.CondominioId != request.CondominioId)
        {
            var novoOk = await _db.Condominios
                .AnyAsync(c => c.Id == request.CondominioId && c.SindicoId == sindicoId);
            if (!novoOk)
                throw new KeyNotFoundException("Condomínio não encontrado");
        }

        entity.CondominioId = request.CondominioId;
        entity.Justificativa = request.Justificativa;
        entity.TipoAprovacao = request.TipoAprovacao;
        entity.AtualizadoEm = DateTime.UtcNow;
        await SincronizarItensAsync(entity, request.Itens);

        await _db.SaveChangesAsync();

        return _mapper.Map<SolicitacaoCompraResponse>(entity);
    }

    public async Task<SolicitacaoCompraDetalheResponse> AprovarAsync(Guid id, Guid userId)
    {
        var aprovador = await _db.Usuarios.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Cargo)
            .FirstOrDefaultAsync();

        if (aprovador != CargoConstants.Sindico)
            throw new UnauthorizedAccessException("Apenas o síndico pode aprovar solicitações de compra");

        var sindicoId = userId;

        var entity = await ComReferencias(_db.SolicitacoesCompra)
            .FirstOrDefaultAsync(s => s.Id == id && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        EnsureStatus(entity, SolicitacaoStatus.Nova, "aprovar");

        if (entity.Cotacoes.Count < 1)
            throw new ValidationException(new[]
            {
                new ValidationFailure("cotacoes", "É necessário ao menos uma cotação")
            });

        if (!entity.Cotacoes.Any(c => c.Selecionada))
            throw new ValidationException(new[]
            {
                new ValidationFailure("cotacoes", "Selecione uma cotação vencedora")
            });

        entity.Status = SolicitacaoStatus.EmAndamento;
        entity.AprovadoPorId = userId;
        entity.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _db.Entry(entity).Reference(s => s.AprovadoPor).LoadAsync();
        if (entity.AprovadoPor is not null)
            await _db.Entry(entity.AprovadoPor).Reference(u => u.Pessoa).LoadAsync();

        return _mapper.Map<SolicitacaoCompraDetalheResponse>(entity);
    }

    public async Task<SolicitacaoCompraResponse> UpdateStatusAsync(
        Guid id, UpdateSolicitacaoCompraStatusRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await ComReferencias(_db.SolicitacoesCompra)
            .FirstOrDefaultAsync(s => s.Id == id && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        if (!TransicaoStatusPermitida(entity.Status, request.Status))
            throw new ValidationException(new[]
            {
                new ValidationFailure("status", $"Transição de status inválida: {entity.Status} → {request.Status}")
            });

        entity.Status = request.Status;
        entity.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<SolicitacaoCompraResponse>(entity);
    }

    private static bool TransicaoStatusPermitida(string atual, string novo)
    {
        if (atual == novo)
            return true;

        return (atual, novo) switch
        {
            (SolicitacaoStatus.Nova, SolicitacaoStatus.Cancelada) => true,
            (SolicitacaoStatus.Cancelada, SolicitacaoStatus.EmAndamento) => true,
            (SolicitacaoStatus.EmAndamento, SolicitacaoStatus.Finalizada) => true,
            (SolicitacaoStatus.EmAndamento, SolicitacaoStatus.Cancelada) => true,
            _ => false
        };
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
            .Include(c => c.Itens)
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
            .Include(s => s.Itens)
            .FirstOrDefaultAsync(s => s.Id == solicitacaoId && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        EnsureStatus(solicitacao, SolicitacaoStatus.Nova, "alterar cotações");
        await ValidarFornecedorAsync(request.FornecedorId, sindicoId);

        var cotacao = new Cotacao
        {
            SolicitacaoCompraId = solicitacao.Id,
            FornecedorId = request.FornecedorId,
            NomeEmpresa = request.FornecedorId.HasValue ? null : request.NomeEmpresa,
            NomeContato = request.NomeContato,
            NomeResponsavel = request.NomeResponsavel,
            FormaPagamento = request.FormaPagamento,
            Selecionada = false,
            CriadoEm = DateTime.UtcNow,
            Itens = MontarItensCotacao(solicitacao, request.Itens)
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
            .Include(c => c.Itens)
            .Include(c => c.SolicitacaoCompra)
            .ThenInclude(s => s.Itens)
            .FirstOrDefaultAsync(c =>
                c.Id == cotacaoId &&
                c.SolicitacaoCompraId == solicitacaoId &&
                c.SolicitacaoCompra.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Cotação não encontrada");

        EnsureStatus(cotacao.SolicitacaoCompra, SolicitacaoStatus.Nova, "alterar cotações");
        await ValidarFornecedorAsync(request.FornecedorId, sindicoId);

        cotacao.FornecedorId = request.FornecedorId;
        cotacao.NomeEmpresa = request.FornecedorId.HasValue ? null : request.NomeEmpresa;
        cotacao.NomeContato = request.NomeContato;
        cotacao.NomeResponsavel = request.NomeResponsavel;
        cotacao.FormaPagamento = request.FormaPagamento;

        await using var tx = await _db.Database.BeginTransactionAsync();
        _db.CotacaoItens.RemoveRange(cotacao.Itens);
        await _db.SaveChangesAsync();

        cotacao.Itens = MontarItensCotacao(cotacao.SolicitacaoCompra, request.Itens);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return _mapper.Map<CotacaoResponse>(cotacao);
    }

    public async Task SelecionarCotacaoAsync(Guid solicitacaoId, Guid cotacaoId, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        await using var tx = await _db.Database.BeginTransactionAsync();

        var solicitacao = await _db.SolicitacoesCompra
            .FirstOrDefaultAsync(s => s.Id == solicitacaoId && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        EnsureStatus(solicitacao, SolicitacaoStatus.Nova, "alterar cotações");

        var cotacoes = await _db.Cotacoes
            .Where(c => c.SolicitacaoCompraId == solicitacaoId)
            .ToListAsync();

        var alvo = cotacoes.FirstOrDefault(c => c.Id == cotacaoId)
            ?? throw new KeyNotFoundException("Cotação não encontrada");

        if (alvo.Selecionada)
        {
            await tx.CommitAsync();
            return;
        }

        foreach (var c in cotacoes.Where(c => c.Selecionada))
            c.Selecionada = false;

        await _db.SaveChangesAsync();

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

        EnsureStatus(cotacao.SolicitacaoCompra, SolicitacaoStatus.Nova, "alterar cotações");

        _db.Cotacoes.Remove(cotacao);
        await _db.SaveChangesAsync();
    }

    private static IQueryable<SolicitacaoCompra> ComReferencias(IQueryable<SolicitacaoCompra> query) =>
        query
            .Include(s => s.Itens)
            .Include(s => s.SolicitadoPor).ThenInclude(u => u.Pessoa)
            .Include(s => s.AprovadoPor!).ThenInclude(u => u.Pessoa)
            .Include(s => s.Cotacoes).ThenInclude(c => c.Itens)
            .Include(s => s.Cotacoes).ThenInclude(c => c.Fornecedor);

    private async Task CarregarAutorAsync(SolicitacaoCompra entity)
    {
        await _db.Entry(entity).Reference(s => s.SolicitadoPor).LoadAsync();
        await _db.Entry(entity.SolicitadoPor).Reference(u => u.Pessoa).LoadAsync();
    }

    private async Task SincronizarItensAsync(SolicitacaoCompra entity, List<SolicitacaoCompraItemRequest> pedidos)
    {
        var idsPedidos = pedidos
            .Where(i => i.Id is Guid id && id != Guid.Empty)
            .Select(i => i.Id!.Value)
            .ToHashSet();

        var remover = entity.Itens.Where(i => !idsPedidos.Contains(i.Id)).ToList();
        if (remover.Count > 0)
        {
            var idsRemover = remover.Select(i => i.Id).ToList();
            var emCotacao = await _db.CotacaoItens.AnyAsync(ci => idsRemover.Contains(ci.ItemId));
            if (emCotacao)
                throw new ValidationException(new[]
                {
                    new ValidationFailure("itens", "Remova as cotações que usam os itens excluídos antes de alterar o pedido.")
                });

            _db.SolicitacaoCompraItens.RemoveRange(remover);
        }

        for (var index = 0; index < pedidos.Count; index++)
        {
            var pedido = pedidos[index];
            if (pedido.Id is Guid id && id != Guid.Empty)
            {
                var item = entity.Itens.FirstOrDefault(i => i.Id == id)
                    ?? throw new ValidationException(new[]
                    {
                        new ValidationFailure("itens", "Item não pertence a esta solicitação.")
                    });

                item.Ordem = index;
                item.Categoria = pedido.Categoria;
                item.Descricao = pedido.Descricao.Trim();
                item.Unidade = string.IsNullOrWhiteSpace(pedido.Unidade) ? null : pedido.Unidade.Trim();
                item.EReposicao = pedido.EReposicao;

                if (item.Quantidade != pedido.Quantidade)
                {
                    item.Quantidade = pedido.Quantidade;
                    var linhas = await _db.CotacaoItens.Where(ci => ci.ItemId == item.Id).ToListAsync();
                    foreach (var linha in linhas)
                        linha.ValorTotal = CalcularValorTotal(linha.ValorUnitario, item.Quantidade);
                }
            }
            else
            {
                entity.Itens.Add(CriarItem(pedido, index));
            }
        }
    }

    private static SolicitacaoCompraItem CriarItem(SolicitacaoCompraItemRequest pedido, int ordem) => new()
    {
        Ordem = ordem,
        Categoria = pedido.Categoria,
        Descricao = pedido.Descricao.Trim(),
        Quantidade = pedido.Quantidade,
        Unidade = string.IsNullOrWhiteSpace(pedido.Unidade) ? null : pedido.Unidade.Trim(),
        EReposicao = pedido.EReposicao,
    };

    private async Task ValidarFornecedorAsync(Guid? fornecedorId, Guid sindicoId)
    {
        if (fornecedorId.HasValue &&
            !await _db.Fornecedores.AnyAsync(f => f.Id == fornecedorId && f.SindicoId == sindicoId))
            throw new KeyNotFoundException("Fornecedor não encontrado");
    }

    private static List<CotacaoItem> MontarItensCotacao(
        SolicitacaoCompra solicitacao, List<CotacaoItemRequest> pedidos)
    {
        var esperados = solicitacao.Itens.Select(i => i.Id).ToHashSet();
        var recebidos = pedidos.Select(p => p.ItemId).ToList();
        if (recebidos.Distinct().Count() != recebidos.Count || !esperados.SetEquals(recebidos))
            throw new ValidationException(new[]
            {
                new ValidationFailure("itens", "Informe o valor de cada item do pedido, uma vez.")
            });

        return pedidos.Select(pedido =>
        {
            var item = solicitacao.Itens.First(i => i.Id == pedido.ItemId);
            return new CotacaoItem
            {
                ItemId = pedido.ItemId,
                ValorUnitario = pedido.ValorUnitario,
                ValorTotal = CalcularValorTotal(pedido.ValorUnitario, item.Quantidade),
            };
        }).ToList();
    }

    private static void EnsureStatus(SolicitacaoCompra s, string esperado, string acao)
    {
        if (s.Status != esperado)
            throw new ValidationException(new[]
            {
                new ValidationFailure("status",
                    $"Não é possível {acao} com status '{s.Status}'. Esperado: '{esperado}'.")
            });
    }

    private static decimal CalcularValorTotal(decimal valorUnitario, decimal quantidade)
        => Math.Round(valorUnitario * quantidade, 2, MidpointRounding.AwayFromZero);
}
