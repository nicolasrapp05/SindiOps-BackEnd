using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class DashboardService : IDashboardService
{
    private readonly SindiOpsDbContext _db;

    public DashboardService(SindiOpsDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardResponse> GetDashboardAsync(Guid userId, Guid? condominioId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        IQueryable<Guid> idsQuery = _db.Condominios.AsNoTracking()
            .Where(c => c.SindicoId == sindicoId)
            .Select(c => c.Id);

        if (condominioId.HasValue)
        {
            var pertence = await _db.Condominios.AsNoTracking()
                .AnyAsync(c => c.Id == condominioId.Value && c.SindicoId == sindicoId);
            if (!pertence)
                throw new KeyNotFoundException("Condomínio não encontrado");

            idsQuery = idsQuery.Where(id => id == condominioId.Value);
        }

        var condominioIds = await idsQuery.ToListAsync();
        if (condominioIds.Count == 0)
            return new DashboardResponse();

        // DbContext não permite consultas concorrentes no mesmo escopo — executar em sequência.
        var alertas = await CarregarAlertasAsync(condominioIds);
        var agenda = await CarregarAgendaAsync(condominioIds, sindicoId, condominioId);

        return new DashboardResponse
        {
            Alertas = alertas,
            Agenda = agenda
        };
    }

    private async Task<DashboardAlertas> CarregarAlertasAsync(List<Guid> condominioIds)
    {
        var manVencidas = _db.ManutencoesObrigatorias.AsNoTracking()
            .Where(m => condominioIds.Contains(m.CondominioId) && m.Status == ManutencaoStatus.Overdue);

        var manProximas = _db.ManutencoesObrigatorias.AsNoTracking()
            .Where(m => condominioIds.Contains(m.CondominioId) && m.Status == ManutencaoStatus.Upcoming);

        var ocorrenciasAbertas = _db.Ocorrencias.AsNoTracking()
            .Where(o => condominioIds.Contains(o.CondominioId) &&
                        (o.Status == OcorrenciaStatus.Nova || o.Status == OcorrenciaStatus.EmAndamento));

        var comprasPendentes = _db.SolicitacoesCompra.AsNoTracking()
            .Where(s => condominioIds.Contains(s.CondominioId) &&
                        (s.Status == SolicitacaoStatus.Nova || s.Status == SolicitacaoStatus.EmAndamento));

        var contratosVencendo = _db.Contratos.AsNoTracking()
            .Where(c => condominioIds.Contains(c.CondominioId) && c.Status == ContratoStatus.Expiring);

        return new DashboardAlertas
        {
            ManutencoesVencidas = await manVencidas.CountAsync(),
            ManutencoesProximas = await manProximas.CountAsync(),
            OcorrenciasAbertas = await ocorrenciasAbertas.CountAsync(),
            ComprasPendentes = await comprasPendentes.CountAsync(),
            ContratosVencendo = await contratosVencendo.CountAsync()
        };
    }

    private async Task<List<AgendaItem>> CarregarAgendaAsync(
        List<Guid> condominioIds,
        Guid sindicoId,
        Guid? condominioIdFiltro)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var limiteMandato = hoje.AddDays(60);

        var manutencoesEntities = await _db.ManutencoesObrigatorias.AsNoTracking()
            .Include(m => m.Condominio)
            .Where(m => condominioIds.Contains(m.CondominioId) &&
                        (m.Status == ManutencaoStatus.Upcoming || m.Status == ManutencaoStatus.Overdue))
            .ToListAsync();

        var manutencoes = manutencoesEntities.Select(m => new AgendaItem
        {
            Tipo = "manutencao_obrigatoria",
            Descricao = m.Tipo + (string.IsNullOrEmpty(m.Observacoes) ? "" : " — " + m.Observacoes),
            DataVencimento = m.DataVencimento,
            Status = m.Status,
            CondominioId = m.CondominioId,
            CondominioNome = m.Condominio.Nome,
            ReferenciaId = m.Id
        }).ToList();

        var contratos = await _db.Contratos.AsNoTracking()
            .Include(c => c.Condominio)
            .Include(c => c.Fornecedor)
            .Where(c => condominioIds.Contains(c.CondominioId) && c.Status == ContratoStatus.Expiring)
            .ToListAsync();

        var agendaContratos = contratos
            .Where(c => c.DataFim.HasValue)
            .Select(c => new AgendaItem
            {
                Tipo = "contrato",
                Descricao = $"{c.TipoServico} — {c.Fornecedor.Nome}",
                DataVencimento = c.DataFim!.Value,
                Status = c.Status,
                CondominioId = c.CondominioId,
                CondominioNome = c.Condominio.Nome,
                ReferenciaId = c.Id
            })
            .ToList();

        var condominiosMandatoQuery = _db.Condominios.AsNoTracking()
            .Where(c => c.SindicoId == sindicoId &&
                        c.VencimentoMandato.HasValue &&
                        c.VencimentoMandato.Value >= hoje &&
                        c.VencimentoMandato.Value <= limiteMandato);

        if (condominioIdFiltro.HasValue)
            condominiosMandatoQuery = condominiosMandatoQuery.Where(c => c.Id == condominioIdFiltro.Value);

        var mandatos = await condominiosMandatoQuery
            .Select(c => new AgendaItem
            {
                Tipo = "mandato",
                Descricao = "Vencimento do mandato do síndico",
                DataVencimento = c.VencimentoMandato!.Value,
                Status = "proximo",
                CondominioId = c.Id,
                CondominioNome = c.Nome,
                ReferenciaId = c.Id
            })
            .ToListAsync();

        var agenda = new List<AgendaItem>(manutencoes.Count + agendaContratos.Count + mandatos.Count);
        agenda.AddRange(manutencoes);
        agenda.AddRange(agendaContratos);
        agenda.AddRange(mandatos);

        agenda.Sort((a, b) => a.DataVencimento.CompareTo(b.DataVencimento));
        return agenda;
    }
}
