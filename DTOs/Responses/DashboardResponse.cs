namespace SindiOps.API.DTOs.Responses;

public class DashboardResponse
{
    public DashboardAlertas Alertas { get; set; } = new();
    public List<AgendaItem> Agenda { get; set; } = [];
}

public class DashboardAlertas
{
    public int? ManutencoesVencidas { get; set; }
    public int? ManutencoesProximas { get; set; }
    public int? OcorrenciasAbertas { get; set; }
    public int? ComprasPendentes { get; set; }
    public int? ContratosVencendo { get; set; }
}

public class AgendaItem
{
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateOnly DataVencimento { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid CondominioId { get; set; }
    public string CondominioNome { get; set; } = string.Empty;
    public Guid ReferenciaId { get; set; }
}
