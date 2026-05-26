namespace SindiCore.API.DTOs.Responses;

public class BlocoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public List<UnidadeResponse> Unidades { get; set; } = [];
}
