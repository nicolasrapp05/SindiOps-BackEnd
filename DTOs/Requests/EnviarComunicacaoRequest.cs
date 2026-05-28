namespace SindiOps.API.DTOs.Requests;

public class EnviarComunicacaoRequest
{
    public Guid TemplateId { get; set; }
    public Guid MoradorId { get; set; }
    public string AssuntoEditado { get; set; } = string.Empty;
    public string CorpoEditado { get; set; } = string.Empty;
    public decimal? ValorMulta { get; set; }
    public string? PrazoResposta { get; set; }
}
