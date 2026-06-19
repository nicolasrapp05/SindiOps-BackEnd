namespace SindiOps.API.DTOs.Responses;

public class PessoaRefResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cargo { get; set; }
}
