namespace SindiOps.API.DTOs.Responses;

public class ContratoDetalheResponse : ContratoResponse
{
    public Guid CondominioId { get; set; }
    public string? NomeContato { get; set; }
    public string? TelefoneContato { get; set; }
    public string? IndiceReajuste { get; set; }
    public string? CondicoesRenovacao { get; set; }
    public string? CondicoesRescisao { get; set; }
}
