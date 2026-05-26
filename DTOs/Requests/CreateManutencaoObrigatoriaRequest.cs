namespace SindiCore.API.DTOs.Requests;

public class CreateManutencaoObrigatoriaRequest
{
    public Guid CondominioId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateOnly DataVencimento { get; set; }
    public DateOnly? UltimaRealizacao { get; set; }
    public string? Observacoes { get; set; }
}
