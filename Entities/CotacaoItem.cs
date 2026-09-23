namespace SindiOps.API.Entities;

public class CotacaoItem
{
    public Guid Id { get; set; }
    public Guid CotacaoId { get; set; }
    public Guid ItemId { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }

    public Cotacao Cotacao { get; set; } = null!;
    public SolicitacaoCompraItem Item { get; set; } = null!;
}
