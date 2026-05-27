namespace SindiOps.API.DTOs.Requests;

public class GerarRelatorioRequest
{
    public string Tipo { get; set; } = string.Empty;
    public Guid CondominioId { get; set; }
    public string Formato { get; set; } = string.Empty;
    public Dictionary<string, string> Filtros { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
