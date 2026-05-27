namespace SindiOps.API.DTOs.Requests;

public class RealizarManutencaoRequest
{
    public DateOnly DataRealizacao { get; set; }
    public string? Observacoes { get; set; }
}
