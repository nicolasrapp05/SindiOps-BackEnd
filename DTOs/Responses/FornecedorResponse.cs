namespace SindiOps.API.DTOs.Responses;

public class FornecedorResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? NomeContato { get; set; }
    public DateTime CriadoEm { get; set; }
}
