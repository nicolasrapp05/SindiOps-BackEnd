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

    public async Task<DashboardResponse> GetDashboardAsync(Guid userId, string cargo, Guid? condominioId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        IQueryable<Guid> idsQuery = _db.Condominios.AsNoTracking()
            .Where(c => c.SindicoId == sindicoId)
            .Select(c => c.Id);

        if (condominioId.HasValue)
        {
            var pertence = await UsuarioSindicoScope.FuncionarioPodeAcessarCondominioAsync(
                _db, userId, sindicoId, condominioId.Value);

            if (!pertence)
                throw new KeyNotFoundException("Condomínio não encontrado");

            idsQuery = idsQuery.Where(id => id == condominioId.Value);
        }
        else
        {
            var acessiveis = await UsuarioSindicoScope.ObterCondominiosAcessiveisAsync(_db, userId, sindicoId);
            idsQuery = idsQuery.Where(id => acessiveis.Contains(id));
        }

        var condominioIds = await idsQuery.ToListAsync();
        if (condominioIds.Count == 0)
            return new DashboardResponse();

        var alertas = await CarregarAlertasAsync(condominioIds, cargo);
        var agenda = await CarregarAgendaAsync(condominioIds, sindicoId, condominioId, cargo);

        return new DashboardResponse
        {
            Alertas = alertas,
            Agenda = agenda
        };
    }

    private async Task<DashboardAlertas> CarregarAlertasAsync(List<Guid> condominioIds, string cargo)
    {
        var alertas = new DashboardAlertas();

        if (CargoPermissions.CanSeeManutencaoAlertas(cargo))
        {
            alertas.ManutencoesVencidas = await _db.ManutencoesObrigatorias.AsNoTracking()
                .CountAsync(m => condominioIds.Contains(m.CondominioId) && m.Status == ManutencaoStatus.Overdue);

            alertas.ManutencoesProximas = await _db.ManutencoesObrigatorias.AsNoTracking()
                .CountAsync(m => condominioIds.Contains(m.CondominioId) && m.Status == ManutencaoStatus.Upcoming);
        }

        if (CargoPermissions.CanSeeOcorrenciaAlertas(cargo))
        {
            alertas.OcorrenciasAbertas = await _db.Ocorrencias.AsNoTracking()
                .CountAsync(o => condominioIds.Contains(o.CondominioId) &&
                    (o.Status == OcorrenciaStatus.Nova || o.Status == OcorrenciaStatus.EmAndamento));
        }

        if (CargoPermissions.CanSeeComprasAlertas(cargo))
        {
            alertas.ComprasPendentes = await _db.SolicitacoesCompra.AsNoTracking()
                .CountAsync(s => condominioIds.Contains(s.CondominioId) &&
                    (s.Status == SolicitacaoStatus.Nova || s.Status == SolicitacaoStatus.EmAndamento));
        }

        if (CargoPermissions.CanSeeContratosAlertas(cargo))
        {
            alertas.ContratosVencendo = await _db.Contratos.AsNoTracking()
                .CountAsync(c => condominioIds.Contains(c.CondominioId) && c.Status == ContratoStatus.Expiring);
        }

        return alertas;
    }

    private async Task<List<AgendaItem>> CarregarAgendaAsync(
        List<Guid> condominioIds,
        Guid sindicoId,
        Guid? condominioIdFiltro,
        string cargo)
    {
        var agenda = new List<AgendaItem>();

        if (CargoPermissions.CanSeeManutencaoAgenda(cargo))
        {
            var manutencoesEntities = await _db.ManutencoesObrigatorias.AsNoTracking()
                .Include(m => m.Condominio)
                .Where(m => condominioIds.Contains(m.CondominioId) &&
                            (m.Status == ManutencaoStatus.Upcoming || m.Status == ManutencaoStatus.Overdue))
                .ToListAsync();

            agenda.AddRange(manutencoesEntities.Select(m => new AgendaItem
            {
                Tipo = "manutencao_obrigatoria",
                Descricao = m.Tipo + (string.IsNullOrEmpty(m.Observacoes) ? "" : " — " + m.Observacoes),
                DataVencimento = m.DataVencimento,
                Status = m.Status,
                CondominioId = m.CondominioId,
                CondominioNome = m.Condominio.Nome,
                ReferenciaId = m.Id
            }));
        }

        if (CargoPermissions.CanSeeContratoAgenda(cargo))
        {
            var contratos = await _db.Contratos.AsNoTracking()
                .Include(c => c.Condominio)
                .Include(c => c.Fornecedor)
                .Where(c => condominioIds.Contains(c.CondominioId) && c.Status == ContratoStatus.Expiring)
                .ToListAsync();

            agenda.AddRange(contratos
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
                }));
        }

        if (CargoPermissions.CanSeeMandatoAgenda(cargo))
        {
            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
            var limiteMandato = hoje.AddDays(60);

            var condominiosMandatoQuery = _db.Condominios.AsNoTracking()
                .Where(c => c.SindicoId == sindicoId &&
                            c.VencimentoMandato.HasValue &&
                            c.VencimentoMandato.Value >= hoje &&
                            c.VencimentoMandato.Value <= limiteMandato &&
                            condominioIds.Contains(c.Id));

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

            agenda.AddRange(mandatos);
        }

        agenda.Sort((a, b) => a.DataVencimento.CompareTo(b.DataVencimento));
        return agenda;
    }
}
