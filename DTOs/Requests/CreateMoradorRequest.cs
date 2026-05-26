namespace SindiCore.API.DTOs.Requests;

public class CreateMoradorRequest
{
    public Guid CondominioId { get; set; }
    public Guid UnidadeId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
}
