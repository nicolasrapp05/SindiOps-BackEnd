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

public class OrcamentoManutencaoService : IOrcamentoManutencaoService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;

    public OrcamentoManutencaoService(SindiOpsDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<OrcamentoManutencaoResponse>> GetAllAsync(Guid solicitacaoId, Guid userId)
    {
        await ExigirSolicitacaoAsync(solicitacaoId, userId);

        var orcamentos = await _db.OrcamentosManutencao
            .Include(o => o.Fornecedor)
            .Where(o => o.SolicitacaoManutencaoId == solicitacaoId)
            .OrderBy(o => o.CriadoEm)
            .ToListAsync();

        return _mapper.Map<List<OrcamentoManutencaoResponse>>(orcamentos);
    }

    public async Task<OrcamentoManutencaoResponse> CreateAsync(
        Guid solicitacaoId, CreateOrcamentoManutencaoRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);
        var solicitacao = await ExigirSolicitacaoAsync(solicitacaoId, sindicoId, tracked: true);
        EnsureEditavel(solicitacao);

        var fornecedorId = await NormalizarFornecedorAsync(request, sindicoId);

        var orcamento = new OrcamentoManutencao
        {
            SolicitacaoManutencaoId = solicitacao.Id,
            FornecedorId = fornecedorId,
            NomeEmpresa = fornecedorId.HasValue ? null : TrimOrNull(request.NomeEmpresa),
            NomeContato = TrimOrNull(request.NomeContato),
            NomeResponsavel = TrimOrNull(request.NomeResponsavel),
            FormaPagamento = TrimOrNull(request.FormaPagamento),
            DescricaoEscopo = TrimOrNull(request.DescricaoEscopo),
            Valor = ArredondarValor(request.Valor),
            PrazoExecucaoDias = request.PrazoExecucaoDias,
            ValidadeProposta = request.ValidadeProposta,
            Garantia = TrimOrNull(request.Garantia),
            Observacoes = TrimOrNull(request.Observacoes),
            Selecionada = false,
            CriadoEm = DateTime.UtcNow,
        };

        _db.OrcamentosManutencao.Add(orcamento);
        await _db.SaveChangesAsync();
        await _db.Entry(orcamento).Reference(o => o.Fornecedor).LoadAsync();

        return _mapper.Map<OrcamentoManutencaoResponse>(orcamento);
    }

    public async Task<OrcamentoManutencaoResponse> UpdateAsync(
        Guid solicitacaoId, Guid orcamentoId, CreateOrcamentoManutencaoRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);
        var orcamento = await ExigirOrcamentoAsync(solicitacaoId, orcamentoId, sindicoId);
        EnsureEditavel(orcamento.SolicitacaoManutencao);

        var fornecedorId = await NormalizarFornecedorAsync(request, sindicoId);

        orcamento.FornecedorId = fornecedorId;
        orcamento.NomeEmpresa = fornecedorId.HasValue ? null : TrimOrNull(request.NomeEmpresa);
        orcamento.NomeContato = TrimOrNull(request.NomeContato);
        orcamento.NomeResponsavel = TrimOrNull(request.NomeResponsavel);
        orcamento.FormaPagamento = TrimOrNull(request.FormaPagamento);
        orcamento.DescricaoEscopo = TrimOrNull(request.DescricaoEscopo);
        orcamento.Valor = ArredondarValor(request.Valor);
        orcamento.PrazoExecucaoDias = request.PrazoExecucaoDias;
        orcamento.ValidadeProposta = request.ValidadeProposta;
        orcamento.Garantia = TrimOrNull(request.Garantia);
        orcamento.Observacoes = TrimOrNull(request.Observacoes);

        await _db.SaveChangesAsync();
        await _db.Entry(orcamento).Reference(o => o.Fornecedor).LoadAsync();

        return _mapper.Map<OrcamentoManutencaoResponse>(orcamento);
    }

    public async Task SelecionarAsync(Guid solicitacaoId, Guid orcamentoId, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        await using var tx = await _db.Database.BeginTransactionAsync();

        var solicitacao = await ExigirSolicitacaoAsync(solicitacaoId, sindicoId, tracked: true);
        EnsureEditavel(solicitacao);

        var orcamentos = await _db.OrcamentosManutencao
            .Where(o => o.SolicitacaoManutencaoId == solicitacaoId)
            .ToListAsync();

        var alvo = orcamentos.FirstOrDefault(o => o.Id == orcamentoId)
            ?? throw new KeyNotFoundException("Orçamento não encontrado");

        if (alvo.Selecionada)
        {
            await tx.CommitAsync();
            return;
        }

        foreach (var orcamento in orcamentos.Where(o => o.Selecionada))
            orcamento.Selecionada = false;

        await _db.SaveChangesAsync();

        alvo.Selecionada = true;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task DeleteAsync(Guid solicitacaoId, Guid orcamentoId, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);
        var orcamento = await ExigirOrcamentoAsync(solicitacaoId, orcamentoId, sindicoId);
        EnsureEditavel(orcamento.SolicitacaoManutencao);

        _db.OrcamentosManutencao.Remove(orcamento);
        await _db.SaveChangesAsync();
    }

    public async Task<SolicitacaoManutencaoResponse> AprovarAsync(Guid solicitacaoId, Guid userId)
    {
        await ExigirSindicoAsync(userId);
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var solicitacao = await ComReferencias(_db.SolicitacoesManutencao)
            .FirstOrDefaultAsync(s => s.Id == solicitacaoId && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        if (solicitacao.Status is not (SolicitacaoStatus.Nova or SolicitacaoStatus.EmAndamento))
            throw Falha("status", "Só é possível aprovar um orçamento de um chamado novo ou em andamento.");

        if (solicitacao.OrcamentoAprovadoPorId != null)
            throw Falha("orcamento", "O orçamento desta manutenção já foi aprovado.");

        if (solicitacao.Orcamentos.Count < 1)
            throw Falha("orcamentos", "É necessário ao menos um orçamento.");

        var selecionado = solicitacao.Orcamentos.SingleOrDefault(o => o.Selecionada)
            ?? throw Falha("orcamentos", "Selecione um orçamento vencedor.");

        if (solicitacao.Status == SolicitacaoStatus.Nova)
            solicitacao.Status = SolicitacaoStatus.EmAndamento;

        solicitacao.OrcamentoAprovadoPorId = userId;
        solicitacao.AtualizadoEm = DateTime.UtcNow;

        if (selecionado.FornecedorId.HasValue)
        {
            solicitacao.FornecedorId = selecionado.FornecedorId;
            solicitacao.Responsavel = ResponsavelSolicitacao.Fornecedor;
        }

        await _db.SaveChangesAsync();
        await CarregarAprovadorAsync(solicitacao);

        return _mapper.Map<SolicitacaoManutencaoResponse>(solicitacao);
    }

    public async Task<SolicitacaoManutencaoResponse> ReabrirAsync(Guid solicitacaoId, Guid userId)
    {
        await ExigirSindicoAsync(userId);
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var solicitacao = await ComReferencias(_db.SolicitacoesManutencao)
            .FirstOrDefaultAsync(s => s.Id == solicitacaoId && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");

        if (solicitacao.Status == SolicitacaoStatus.Finalizada)
            throw Falha("status", "Não é possível reabrir o orçamento de um chamado finalizado.");

        if (solicitacao.OrcamentoAprovadoPorId == null)
            throw Falha("orcamento", "O orçamento desta manutenção não está aprovado.");

        solicitacao.OrcamentoAprovadoPorId = null;
        solicitacao.OrcamentoAprovadoPor = null;
        solicitacao.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return _mapper.Map<SolicitacaoManutencaoResponse>(solicitacao);
    }

    private async Task ExigirSolicitacaoAsync(Guid solicitacaoId, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);
        await ExigirSolicitacaoAsync(solicitacaoId, sindicoId, tracked: false);
    }

    private async Task<SolicitacaoManutencao> ExigirSolicitacaoAsync(Guid solicitacaoId, Guid sindicoId, bool tracked)
    {
        var query = _db.SolicitacoesManutencao.AsQueryable();
        if (!tracked)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(s => s.Id == solicitacaoId && s.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Solicitação não encontrada");
    }

    private async Task<OrcamentoManutencao> ExigirOrcamentoAsync(Guid solicitacaoId, Guid orcamentoId, Guid sindicoId)
    {
        return await _db.OrcamentosManutencao
            .Include(o => o.Fornecedor)
            .Include(o => o.SolicitacaoManutencao)
            .FirstOrDefaultAsync(o =>
                o.Id == orcamentoId &&
                o.SolicitacaoManutencaoId == solicitacaoId &&
                o.SolicitacaoManutencao.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Orçamento não encontrado");
    }

    private async Task<Guid?> NormalizarFornecedorAsync(CreateOrcamentoManutencaoRequest request, Guid sindicoId)
    {
        if (request.Valor <= 0)
            throw Falha("valor", "Valor deve ser maior que zero.");

        if (request.PrazoExecucaoDias is <= 0)
            throw Falha("prazoExecucaoDias", "PrazoExecucaoDias deve ser maior que zero.");

        var fornecedorId = request.FornecedorId is Guid id && id != Guid.Empty ? id : (Guid?)null;
        if (!fornecedorId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(request.NomeEmpresa))
                throw Falha("nomeEmpresa", "NomeEmpresa é obrigatório quando FornecedorId não é informado.");
            return null;
        }

        var pertence = await _db.Fornecedores.AnyAsync(f => f.Id == fornecedorId && f.SindicoId == sindicoId);
        if (!pertence)
            throw Falha("fornecedorId", "Fornecedor não encontrado.");

        return fornecedorId;
    }

    private async Task ExigirSindicoAsync(Guid userId)
    {
        var ehSindico = await _db.Sindicos.AsNoTracking().AnyAsync(s => s.UsuarioId == userId);
        if (!ehSindico)
            throw new UnauthorizedAccessException("Apenas o síndico pode aprovar ou reabrir orçamentos de manutenção.");
    }

    private async Task CarregarAprovadorAsync(SolicitacaoManutencao solicitacao)
    {
        await _db.Entry(solicitacao).Reference(s => s.OrcamentoAprovadoPor).LoadAsync();
        if (solicitacao.OrcamentoAprovadoPor is not null)
            await UsuarioPapeis.CarregarAsync(_db, solicitacao.OrcamentoAprovadoPor);

        if (solicitacao.FornecedorId is Guid fornecedorId)
            solicitacao.Fornecedor = await _db.Fornecedores.FirstAsync(f => f.Id == fornecedorId);
        else
            solicitacao.Fornecedor = null;
    }

    private static IQueryable<SolicitacaoManutencao> ComReferencias(IQueryable<SolicitacaoManutencao> query) =>
        query
            .Include(s => s.Fornecedor)
            .Include(s => s.Orcamentos)
            .Include(s => s.OrcamentoAprovadoPor!).ThenInclude(u => u.Sindico)
            .Include(s => s.OrcamentoAprovadoPor!).ThenInclude(u => u.Funcionario)
            .Include(s => s.SolicitadoPor).ThenInclude(u => u.Sindico)
            .Include(s => s.SolicitadoPor).ThenInclude(u => u.Funcionario);

    private static void EnsureEditavel(SolicitacaoManutencao solicitacao)
    {
        if (solicitacao.Status is not (SolicitacaoStatus.Nova or SolicitacaoStatus.EmAndamento))
            throw Falha("status", $"Não é possível alterar orçamentos com status '{solicitacao.Status}'.");

        if (solicitacao.OrcamentoAprovadoPorId != null)
            throw Falha("orcamento", "O orçamento já foi aprovado. Reabra o processo para alterar as propostas.");
    }

    private static ValidationException Falha(string campo, string mensagem) =>
        new(new[] { new ValidationFailure(campo, mensagem) });

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal ArredondarValor(decimal valor) =>
        Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
