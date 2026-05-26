namespace SindiCore.API.DTOs.Requests;

public class UpdateMoradorRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public Guid UnidadeId { get; set; }
}
