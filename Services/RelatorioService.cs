using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Infrastructure.Reports;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class RelatorioService : IRelatorioService
{
    private readonly SindiOpsDbContext _db;
    private readonly IReportGenerator _reportGenerator;

    public RelatorioService(SindiOpsDbContext db, IReportGenerator reportGenerator)
    {
        _db = db;
        _reportGenerator = reportGenerator;
    }

    public async Task<(byte[] Conteudo, string ContentType, string FileName)> GerarRelatorioAsync(
        GerarRelatorioRequest request,
        Guid userId,
        string cargo)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);
        var ids = await UsuarioSindicoScope.ResolverIdsCondominioAsync(
            _db, userId, sindicoId, request.CondominioId);
        var todos = !request.CondominioId.HasValue;

        var doc = request.Tipo switch
        {
            RelatorioTipo.Ocorrencias => await BuildOcorrenciasAsync(request, ids, todos),
            RelatorioTipo.MapaCotacoes => await BuildMapaCotacoesAsync(request, ids, todos),
            RelatorioTipo.ListaCompras => await BuildListaComprasAsync(request, ids, todos),
            RelatorioTipo.AgendaPrazos => await BuildAgendaPrazosAsync(request, cargo, ids, todos),
            RelatorioTipo.AgendaMandatos => await BuildAgendaMandatosAsync(ids),
            RelatorioTipo.Manutencoes => await BuildManutencoesAsync(request, ids, todos),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Tipo), request.Tipo, "Tipo de relatório inválido.")
        };

        var bytes = await _reportGenerator.GenerateAsync(request.Tipo, doc, request.Formato);

        var contentType = request.Formato.ToLowerInvariant() switch
        {
            RelatorioFormato.Pdf => "application/pdf",
            RelatorioFormato.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            RelatorioFormato.Word => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };

        var ext = request.Formato.ToLowerInvariant() switch
        {
            RelatorioFormato.Pdf => "pdf",
            RelatorioFormato.Excel => "xlsx",
            RelatorioFormato.Word => "docx",
            _ => "bin"
        };

        var fileName = $"relatorio_{request.Tipo}_{DateTime.UtcNow:yyyy-MM-dd}.{ext}";
        return (bytes, contentType, fileName);
    }

    private async Task<ReportDocumentModel> BuildOcorrenciasAsync(
        GerarRelatorioRequest request, IReadOnlyList<Guid> ids, bool todos)
    {
        var (ini, fim) = ParseIntervaloUtc(request.Filtros);
        var statusFiltro = GetFiltro(request.Filtros, "status");

        var query = _db.Ocorrencias.AsNoTracking()
            .Include(o => o.Condominio)
            .Where(o => ids.Contains(o.CondominioId));

        if (ini.HasValue)
            query = query.Where(o => o.OcorreuEm >= ini.Value);
        if (fim.HasValue)
            query = query.Where(o => o.OcorreuEm <= fim.Value);
        if (!string.IsNullOrWhiteSpace(statusFiltro))
            query = query.Where(o => o.Status == statusFiltro);

        var list = await query.OrderByDescending(o => o.OcorreuEm).ToListAsync();

        var colunas = Colunas(todos, "Ocorreu em", "Tipo", "Status", "Origem", "Descrição");
        var linhas = list.Select(o => Linha(
            todos,
            o.Condominio.Nome,
            BrazilTime.ToBrazil(o.OcorreuEm).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
            o.TipoOcorrencia,
            o.Status,
            o.Origem,
            Truncar(o.Descricao, 200)
        )).ToList();

        return new ReportDocumentModel
        {
            Titulo = "Ocorrências",
            Periodo = MontarPeriodo(ini, fim),
            Colunas = colunas,
            Linhas = GarantirLinhas(colunas.Length, linhas)
        };
    }

    private async Task<ReportDocumentModel> BuildMapaCotacoesAsync(
        GerarRelatorioRequest request, IReadOnlyList<Guid> ids, bool todos)
    {
        var (ini, fim) = ParseIntervaloUtc(request.Filtros);

        var query = _db.Cotacoes.AsNoTracking()
            .Include(c => c.Itens).ThenInclude(i => i.Item)
            .Include(c => c.SolicitacaoCompra).ThenInclude(s => s.Condominio)
            .Include(c => c.Fornecedor)
            .Where(c => ids.Contains(c.SolicitacaoCompra.CondominioId));

        if (ini.HasValue)
            query = query.Where(c => c.CriadoEm >= ini.Value);
        if (fim.HasValue)
            query = query.Where(c => c.CriadoEm <= fim.Value);

        var list = await query.OrderByDescending(c => c.CriadoEm).ToListAsync();

        var colunas = Colunas(
            todos, "Item", "Categoria", "Fornecedor", "Valor unit.", "Valor total", "Selecionada", "Criado em");

        var linhas = list.SelectMany(c =>
        {
            var fornecedorNome = c.Fornecedor?.Nome ?? c.NomeEmpresa ?? "—";
            return c.Itens.Select(linha => Linha(
                todos,
                c.SolicitacaoCompra.Condominio.Nome,
                linha.Item.Descricao,
                linha.Item.Categoria,
                fornecedorNome,
                linha.ValorUnitario.ToString("F2", CultureInfo.InvariantCulture),
                linha.ValorTotal.ToString("F2", CultureInfo.InvariantCulture),
                c.Selecionada ? "Sim" : "Não",
                c.CriadoEm.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) + " UTC"
            ));
        }).ToList();

        return new ReportDocumentModel
        {
            Titulo = "Mapa de cotações",
            Periodo = MontarPeriodo(ini, fim),
            Colunas = colunas,
            Linhas = GarantirLinhas(colunas.Length, linhas)
        };
    }

    private async Task<ReportDocumentModel> BuildListaComprasAsync(
        GerarRelatorioRequest request, IReadOnlyList<Guid> ids, bool todos)
    {
        var (ini, fim) = ParseIntervaloUtc(request.Filtros);
        var statusFiltro = GetFiltro(request.Filtros, "status");

        var query = _db.SolicitacoesCompra.AsNoTracking()
            .Include(s => s.Condominio)
            .Include(s => s.Itens)
            .Where(s => ids.Contains(s.CondominioId));

        if (ini.HasValue)
            query = query.Where(s => s.CriadoEm >= ini.Value);
        if (fim.HasValue)
            query = query.Where(s => s.CriadoEm <= fim.Value);
        if (!string.IsNullOrWhiteSpace(statusFiltro))
            query = query.Where(s => s.Status == statusFiltro);

        var list = await query.OrderByDescending(s => s.CriadoEm).ToListAsync();

        var colunas = Colunas(todos, "Item", "Categoria", "Quantidade", "Status", "Reposição", "Criado em");
        var linhas = list.SelectMany(s => s.Itens.Select(item => Linha(
            todos,
            s.Condominio.Nome,
            item.Descricao,
            item.Categoria,
            item.Quantidade.ToString("F2", CultureInfo.InvariantCulture),
            s.Status,
            item.EReposicao ? "Sim" : "Não",
            s.CriadoEm.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) + " UTC"
        ))).ToList();

        return new ReportDocumentModel
        {
            Titulo = "Lista de compras",
            Periodo = MontarPeriodo(ini, fim),
            Colunas = colunas,
            Linhas = GarantirLinhas(colunas.Length, linhas)
        };
    }

    private async Task<ReportDocumentModel> BuildAgendaPrazosAsync(
        GerarRelatorioRequest request, string cargo, IReadOnlyList<Guid> ids, bool todos)
    {
        var colunas = Colunas(todos, "Tipo", "Descrição", "Data vencimento", "Status", "Referência (Id)");
        var itens = new List<(DateOnly Data, string Tipo, string Status, string[] Linha)>();
        var tipoFiltro = GetFiltro(request.Filtros, "tipo");
        var statusFiltro = GetFiltro(request.Filtros, "status");

        if (IncluiTipoAgenda(tipoFiltro, "manutencao_obrigatoria") &&
            CargoPermissions.CanSeeManutencaoAgenda(cargo))
        {
            var manutencoes = await _db.ManutencoesObrigatorias.AsNoTracking()
                .Include(m => m.Condominio)
                .Where(m => ids.Contains(m.CondominioId) &&
                            (m.Status == ManutencaoStatus.Upcoming || m.Status == ManutencaoStatus.Overdue))
                .ToListAsync();

            foreach (var m in manutencoes)
            {
                var desc = m.Tipo + (string.IsNullOrEmpty(m.Observacoes) ? "" : " — " + m.Observacoes);
                itens.Add((m.DataVencimento, "manutencao_obrigatoria", m.Status, Linha(
                    todos,
                    m.Condominio.Nome,
                    "Manutenção obrigatória",
                    desc,
                    m.DataVencimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    m.Status,
                    m.Id.ToString()
                ).ToArray()));
            }
        }

        if (IncluiTipoAgenda(tipoFiltro, "contrato") && CargoPermissions.CanSeeContratoAgenda(cargo))
        {
            var contratos = await _db.Contratos.AsNoTracking()
                .Include(c => c.Condominio)
                .Include(c => c.Fornecedor)
                .Where(c => ids.Contains(c.CondominioId) &&
                            c.Status == ContratoStatus.Expiring &&
                            c.DataFim.HasValue)
                .ToListAsync();

            foreach (var c in contratos)
            {
                itens.Add((c.DataFim!.Value, "contrato", c.Status, Linha(
                    todos,
                    c.Condominio.Nome,
                    "Contrato",
                    $"{c.TipoServico} — {c.Fornecedor.Nome}",
                    c.DataFim.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    c.Status,
                    c.Id.ToString()
                ).ToArray()));
            }
        }

        if (IncluiTipoAgenda(tipoFiltro, "mandato") && CargoPermissions.CanSeeMandatoAgenda(cargo))
        {
            var hoje = BrazilTime.Today;
            var limite = hoje.AddDays(MandatoAgenda.WindowDays);
            var condominios = await _db.Condominios.AsNoTracking()
                .Where(c => ids.Contains(c.Id) &&
                            c.VencimentoMandato.HasValue &&
                            c.VencimentoMandato.Value <= limite)
                .Select(c => new { c.Nome, Vencimento = c.VencimentoMandato!.Value, c.Id })
                .ToListAsync();

            foreach (var condominio in condominios)
            {
                var status = MandatoAgenda.Status(condominio.Vencimento, hoje);
                if (status is null)
                    continue;

                itens.Add((condominio.Vencimento, "mandato", status, Linha(
                    todos,
                    condominio.Nome,
                    "Mandato",
                    "Vencimento do mandato do síndico",
                    condominio.Vencimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    status,
                    condominio.Id.ToString()
                ).ToArray()));
            }
        }

        var linhas = itens
            .Where(i => string.IsNullOrWhiteSpace(statusFiltro) || i.Status == statusFiltro)
            .OrderBy(i => i.Data)
            .Select(i => (IReadOnlyList<string>)i.Linha)
            .ToList();

        return new ReportDocumentModel
        {
            Titulo = "Agenda de prazos",
            Periodo = null,
            Colunas = colunas,
            Linhas = GarantirLinhas(colunas.Length, linhas)
        };
    }

    private static bool IncluiTipoAgenda(string? tipoFiltro, string tipo) =>
        string.IsNullOrWhiteSpace(tipoFiltro) || tipoFiltro == "all" || tipoFiltro == tipo;

    private async Task<ReportDocumentModel> BuildAgendaMandatosAsync(IReadOnlyList<Guid> ids)
    {
        var condominios = await _db.Condominios.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .OrderBy(x => x.Nome)
            .ToListAsync();

        var colunas = new[] { "Condomínio", "Data eleição", "Vencimento mandato" };
        var linhas = condominios.Select(c => (IReadOnlyList<string>)new[]
        {
            c.Nome,
            c.DataEleicao?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "—",
            c.VencimentoMandato?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "—"
        }).ToList();

        return new ReportDocumentModel
        {
            Titulo = "Agenda de mandatos",
            Periodo = null,
            Colunas = colunas,
            Linhas = GarantirLinhas(colunas.Length, linhas)
        };
    }

    private async Task<ReportDocumentModel> BuildManutencoesAsync(
        GerarRelatorioRequest request, IReadOnlyList<Guid> ids, bool todos)
    {
        var statusFiltro = GetFiltro(request.Filtros, "status");

        var query = _db.ManutencoesObrigatorias.AsNoTracking()
            .Include(m => m.Condominio)
            .Where(m => ids.Contains(m.CondominioId));

        if (!string.IsNullOrWhiteSpace(statusFiltro))
            query = query.Where(m => m.Status == statusFiltro);

        var list = await query.OrderBy(m => m.DataVencimento).ToListAsync();

        var colunas = Colunas(todos, "Tipo", "Data vencimento", "Última realização", "Status", "Observações");
        var linhas = list.Select(m => Linha(
            todos,
            m.Condominio.Nome,
            m.Tipo,
            m.DataVencimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            m.UltimaRealizacao?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "—",
            m.Status,
            m.Observacoes ?? "—"
        )).ToList();

        return new ReportDocumentModel
        {
            Titulo = "Manutenções obrigatórias",
            Periodo = null,
            Colunas = colunas,
            Linhas = GarantirLinhas(colunas.Length, linhas)
        };
    }

    private static string[] Colunas(bool todos, params string[] colunas) =>
        todos ? ["Condomínio", ..colunas] : colunas;

    private static IReadOnlyList<string> Linha(bool todos, string? condominio, params string[] campos) =>
        todos ? [string.IsNullOrWhiteSpace(condominio) ? "—" : condominio, ..campos] : campos;

    private static string? GetFiltro(Dictionary<string, string> filtros, string key)
    {
        if (!filtros.TryGetValue(key, out var v))
            return null;
        return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    }

    private static (DateTime? Inicio, DateTime? Fim) ParseIntervaloUtc(Dictionary<string, string> filtros)
    {
        DateTime? ini = null;
        DateTime? fim = null;

        if (filtros.TryGetValue("dataInicio", out var sIni) && TryParseDateOnly(sIni, out var dIni))
            ini = BrazilTime.StartOfDayUtc(dIni);

        if (filtros.TryGetValue("dataFim", out var sFim) && TryParseDateOnly(sFim, out var dFim))
            fim = BrazilTime.EndOfDayUtc(dFim);

        return (ini, fim);
    }

    private static bool TryParseDateOnly(string? s, out DateOnly d)
    {
        d = default;
        if (string.IsNullOrWhiteSpace(s))
            return false;

        return DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)
               || DateOnly.TryParse(s, CultureInfo.GetCultureInfo("pt-BR"), DateTimeStyles.None, out d);
    }

    private static string? MontarPeriodo(DateTime? ini, DateTime? fim)
    {
        if (!ini.HasValue && !fim.HasValue)
            return null;
        var a = ini.HasValue
            ? DateOnly.FromDateTime(BrazilTime.ToBrazil(ini.Value)).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            : "…";
        var b = fim.HasValue
            ? DateOnly.FromDateTime(BrazilTime.ToBrazil(fim.Value)).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            : "…";
        return $"Período: {a} a {b}";
    }

    private static string Truncar(string texto, int max)
    {
        if (string.IsNullOrEmpty(texto) || texto.Length <= max)
            return texto;
        return texto[..max] + "…";
    }

    private static List<IReadOnlyList<string>> GarantirLinhas(int colunas, List<IReadOnlyList<string>> linhas)
    {
        if (linhas.Count > 0)
            return linhas;

        var vazia = new string[colunas];
        for (var i = 0; i < colunas; i++)
            vazia[i] = i == 0 ? "Sem registos" : "—";
        return new List<IReadOnlyList<string>> { vazia };
    }
}
