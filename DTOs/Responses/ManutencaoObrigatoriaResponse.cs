namespace SindiOps.API.DTOs.Responses;

public class ManutencaoObrigatoriaResponse
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateOnly DataVencimento { get; set; }
    public DateOnly? UltimaRealizacao { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public CondominioRefResponse Condominio { get; set; } = null!;
}
