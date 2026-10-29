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

        var condominioOk = await _db.Condominios.AsNoTracking()
            .AnyAsync(c => c.Id == request.CondominioId && c.SindicoId == sindicoId);
        if (!condominioOk)
            throw new KeyNotFoundException("Condomínio não encontrado");

        var doc = request.Tipo switch
        {
            RelatorioTipo.Ocorrencias => await BuildOcorrenciasAsync(request),
            RelatorioTipo.MapaCotacoes => await BuildMapaCotacoesAsync(request),
            RelatorioTipo.ListaCompras => await BuildListaComprasAsync(request),
            RelatorioTipo.AgendaPrazos => await BuildAgendaPrazosAsync(request, cargo),
            RelatorioTipo.AgendaMandatos => await BuildAgendaMandatosAsync(request),
            RelatorioTipo.Manutencoes => await BuildManutencoesAsync(request),
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

    private async Task<ReportDocumentModel> BuildOcorrenciasAsync(GerarRelatorioRequest request)
    {
        var (ini, fim) = ParseIntervaloUtc(request.Filtros);
        var statusFiltro = GetFiltro(request.Filtros, "status");

        var query = _db.Ocorrencias.AsNoTracking()
            .Where(o => o.CondominioId == request.CondominioId);

        if (ini.HasValue)
            query = query.Where(o => o.OcorreuEm >= ini.Value);
        if (fim.HasValue)
            query = query.Where(o => o.OcorreuEm <= fim.Value);
        if (!string.IsNullOrWhiteSpace(statusFiltro))
            query = query.Where(o => o.Status == statusFiltro);

        var list = await query.OrderByDescending(o => o.OcorreuEm).ToListAsync();

        var colunas = new[] { "Ocorreu em", "Tipo", "Status", "Origem", "Descrição" };
        var linhas = list.Select(o => (IReadOnlyList<string>)new[]
        {
            BrazilTime.ToBrazil(o.OcorreuEm).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
            o.TipoOcorrencia,
            o.Status,
            o.Origem,
            Truncar(o.Descricao, 200)
        }).ToList();

        return new ReportDocumentModel
        {
            Titulo = "Ocorrências",
            Periodo = MontarPeriodo(ini, fim),
            Colunas = colunas,
            Linhas = GarantirLinhas(colunas.Length, linhas)
        };
    }

    private async Task<ReportDocumentModel> BuildMapaCotacoesAsync(GerarRelatorioRequest request)
    {
        var (ini, fim) = ParseIntervaloUtc(request.Filtros);

        var query = _db.Cotacoes.AsNoTracking()
            .Include(c => c.Itens).ThenInclude(i => i.Item)
            .Include(c => c.SolicitacaoCompra)
            .Include(c => c.Fornecedor)
            .Where(c => c.SolicitacaoCompra.CondominioId == request.CondominioId);

        if (ini.HasValue)
            query = query.Where(c => c.CriadoEm >= ini.Value);
        if (fim.HasValue)
            query = query.Where(c => c.CriadoEm <= fim.Value);

        var list = await query.OrderByDescending(c => c.CriadoEm).ToListAsync();

        var colunas = new[]
        {
            "Item", "Categoria", "Fornecedor", "Valor unit.", "Valor total", "Selecionada", "Criado em"
        };

        var linhas = list.SelectMany(c =>
        {
            var fornecedorNome = c.Fornecedor?.Nome ?? c.NomeEmpresa ?? "—";
            return c.Itens.Select(linha => (IReadOnlyList<string>)new[]
            {
                linha.Item.Descricao,
                linha.Item.Categoria,
                fornecedorNome,
                linha.ValorUnitario.ToString("F2", CultureInfo.InvariantCulture),
                linha.ValorTotal.ToString("F2", CultureInfo.InvariantCulture),
                c.Selecionada ? "Sim" : "Não",
                c.CriadoEm.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) + " UTC"
            });
        }).ToList();

        return new ReportDocumentModel
        {
            Titulo = "Mapa de cotações",
            Periodo = MontarPeriodo(ini, fim),
            Colunas = colunas,
            Linhas = GarantirLinhas(colunas.Length, linhas)
        };
    }

    private async Task<ReportDocumentModel> BuildListaComprasAsync(GerarRelatorioRequest request)
    {
        var (ini, fim) = ParseIntervaloUtc(request.Filtros);
        var statusFiltro = GetFiltro(request.Filtros, "status");

        var query = _db.SolicitacoesCompra.AsNoTracking()
            .Include(s => s.Itens)
            .Where(s => s.CondominioId == request.CondominioId);

        if (ini.HasValue)
            query = query.Where(s => s.CriadoEm >= ini.Value);
        if (fim.HasValue)
            query = query.Where(s => s.CriadoEm <= fim.Value);
        if (!string.IsNullOrWhiteSpace(statusFiltro))
            query = query.Where(s => s.Status == statusFiltro);

        var list = await query.OrderByDescending(s => s.CriadoEm).ToListAsync();

        var colunas = new[] { "Item", "Categoria", "Quantidade", "Status", "Reposição", "Criado em" };
        var linhas = list.SelectMany(s => s.Itens.Select(item => (IReadOnlyList<string>)new[]
        {
            item.Descricao,
            item.Categoria,
            item.Quantidade.ToString("F2", CultureInfo.InvariantCulture),
            s.Status,
            item.EReposicao ? "Sim" : "Não",
            s.CriadoEm.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) + " UTC"
        })).ToList();

        return new ReportDocumentModel
        {
            Titulo = "Lista de compras",
            Periodo = MontarPeriodo(ini, fim),
            Colunas = colunas,
            Linhas = GarantirLinhas(colunas.Length, linhas)
        };
    }

    private async Task<ReportDocumentModel> BuildAgendaPrazosAsync(GerarRelatorioRequest request, string cargo)
    {
        var colunas = new[] { "Tipo", "Descrição", "Data vencimento", "Status", "Referência (Id)" };
        var itens = new List<(DateOnly Data, string Tipo, string Status, string[] Linha)>();
        var tipoFiltro = GetFiltro(request.Filtros, "tipo");
        var statusFiltro = GetFiltro(request.Filtros, "status");

        if (IncluiTipoAgenda(tipoFiltro, "manutencao_obrigatoria") &&
            CargoPermissions.CanSeeManutencaoAgenda(cargo))
        {
            var manutencoes = await _db.ManutencoesObrigatorias.AsNoTracking()
                .Where(m => m.CondominioId == request.CondominioId &&
                            (m.Status == ManutencaoStatus.Upcoming || m.Status == ManutencaoStatus.Overdue))
                .ToListAsync();

            foreach (var m in manutencoes)
            {
                var desc = m.Tipo + (string.IsNullOrEmpty(m.Observacoes) ? "" : " — " + m.Observacoes);
                itens.Add((m.DataVencimento, "manutencao_obrigatoria", m.Status, new[]
                {
                    "Manutenção obrigatória",
                    desc,
                    m.DataVencimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    m.Status,
                    m.Id.ToString()
                }));
            }
        }

        if (IncluiTipoAgenda(tipoFiltro, "contrato") && CargoPermissions.CanSeeContratoAgenda(cargo))
        {
            var contratos = await _db.Contratos.AsNoTracking()
                .Include(c => c.Fornecedor)
                .Where(c => c.CondominioId == request.CondominioId &&
                            c.Status == ContratoStatus.Expiring &&
                            c.DataFim.HasValue)
                .ToListAsync();

            foreach (var c in contratos)
            {
                itens.Add((c.DataFim!.Value, "contrato", c.Status, new[]
                {
                    "Contrato",
                    $"{c.TipoServico} — {c.Fornecedor.Nome}",
                    c.DataFim.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    c.Status,
                    c.Id.ToString()
                }));
            }
        }

        if (IncluiTipoAgenda(tipoFiltro, "mandato") && CargoPermissions.CanSeeMandatoAgenda(cargo))
        {
            var hoje = BrazilTime.Today;
            var limite = hoje.AddDays(MandatoAgenda.WindowDays);
            var condominio = await _db.Condominios.AsNoTracking()
                .Where(c => c.Id == request.CondominioId &&
                            c.VencimentoMandato.HasValue &&
                            c.VencimentoMandato.Value <= limite)
                .Select(c => new { c.Nome, Vencimento = c.VencimentoMandato!.Value, c.Id })
                .FirstOrDefaultAsync();

            if (condominio is not null)
            {
                var status = MandatoAgenda.Status(condominio.Vencimento, hoje);
                if (status is not null)
                {
                    itens.Add((condominio.Vencimento, "mandato", status, new[]
                    {
                        "Mandato",
                        "Vencimento do mandato do síndico",
                        condominio.Vencimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                        status,
                        condominio.Id.ToString()
                    }));
                }
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

    private async Task<ReportDocumentModel> BuildAgendaMandatosAsync(GerarRelatorioRequest request)
    {
        var c = await _db.Condominios.AsNoTracking()
            .FirstAsync(x => x.Id == request.CondominioId);

        var colunas = new[] { "Condomínio", "Data eleição", "Vencimento mandato" };
        var linhas = new List<IReadOnlyList<string>>
        {
            new[]
            {
                c.Nome,
                c.DataEleicao?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "—",
                c.VencimentoMandato?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "—"
            }
        };

        return new ReportDocumentModel
        {
            Titulo = "Agenda de mandatos",
            Periodo = null,
            Colunas = colunas,
            Linhas = linhas
        };
    }

    private async Task<ReportDocumentModel> BuildManutencoesAsync(GerarRelatorioRequest request)
    {
        var statusFiltro = GetFiltro(request.Filtros, "status");

        var query = _db.ManutencoesObrigatorias.AsNoTracking()
            .Where(m => m.CondominioId == request.CondominioId);

        if (!string.IsNullOrWhiteSpace(statusFiltro))
            query = query.Where(m => m.Status == statusFiltro);

        var list = await query.OrderBy(m => m.DataVencimento).ToListAsync();

        var colunas = new[] { "Tipo", "Data vencimento", "Última realização", "Status", "Observações" };
        var linhas = list.Select(m => (IReadOnlyList<string>)new[]
        {
            m.Tipo,
            m.DataVencimento.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            m.UltimaRealizacao?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "—",
            m.Status,
            m.Observacoes ?? "—"
        }).ToList();

        return new ReportDocumentModel
        {
            Titulo = "Manutenções obrigatórias",
            Periodo = null,
            Colunas = colunas,
            Linhas = GarantirLinhas(colunas.Length, linhas)
        };
    }

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
