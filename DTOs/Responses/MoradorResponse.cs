namespace SindiCore.API.DTOs.Responses;

public class MoradorResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public BlocoRefResponse Bloco { get; set; } = null!;
    public UnidadeResponse Unidade { get; set; } = null!;
    public DateTime CriadoEm { get; set; }
}
