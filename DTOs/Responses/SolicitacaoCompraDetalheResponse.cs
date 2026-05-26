namespace SindiCore.API.DTOs.Responses;

public class SolicitacaoCompraDetalheResponse : SolicitacaoCompraResponse
{
    public Guid CondominioId { get; set; }
    public string? Justificativa { get; set; }
    public List<CotacaoResponse> Cotacoes { get; set; } = [];
}
