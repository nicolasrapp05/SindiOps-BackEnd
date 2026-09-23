namespace SindiOps.API.Entities;

public class SolicitacaoCompraItem
{
    public Guid Id { get; set; }
    public Guid SolicitacaoCompraId { get; set; }
    public int Ordem { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public string? Unidade { get; set; }
    public bool EReposicao { get; set; }

    public SolicitacaoCompra SolicitacaoCompra { get; set; } = null!;
    public ICollection<CotacaoItem> CotacaoItens { get; set; } = [];
}
