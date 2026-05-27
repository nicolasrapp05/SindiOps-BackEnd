namespace SindiOps.API.DTOs.Requests;

public class CreateContratoRequest
{
    public Guid CondominioId { get; set; }
    public Guid FornecedorId { get; set; }
    public string TipoServico { get; set; } = string.Empty;
    public string? NomeContato { get; set; }
    public string? TelefoneContato { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public decimal? ValorMensal { get; set; }
    public string? IndiceReajuste { get; set; }
    public string? CondicoesRenovacao { get; set; }
    public string? CondicoesRescisao { get; set; }
}
