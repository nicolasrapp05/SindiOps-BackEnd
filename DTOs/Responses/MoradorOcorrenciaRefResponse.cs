namespace SindiCore.API.DTOs.Responses;

public class MoradorOcorrenciaRefResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public UnidadeNumeroRefResponse Unidade { get; set; } = null!;
}
