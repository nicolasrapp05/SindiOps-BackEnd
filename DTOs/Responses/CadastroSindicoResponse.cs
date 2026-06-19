namespace SindiOps.API.DTOs.Responses;

public class CadastroSindicoResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
