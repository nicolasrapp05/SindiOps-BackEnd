namespace SindiCore.API.DTOs.Responses;

public class ServicoFornecedorResponse
{
    public Guid Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal? Quantidade { get; set; }
}
