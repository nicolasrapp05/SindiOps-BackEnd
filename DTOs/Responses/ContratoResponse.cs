namespace SindiOps.API.DTOs.Responses;

public class ContratoResponse
{
    public Guid Id { get; set; }
    public string TipoServico { get; set; } = string.Empty;
    public FornecedorRefResponse Fornecedor { get; set; } = null!;
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public decimal? ValorMensal { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}
