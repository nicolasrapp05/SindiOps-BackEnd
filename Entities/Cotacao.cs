namespace SindiOps.API.Entities;

public class Cotacao
{
    public Guid Id { get; set; }
    public Guid SolicitacaoCompraId { get; set; }
    public Guid? FornecedorId { get; set; }
    public string? NomeEmpresa { get; set; }
    public string? NomeContato { get; set; }
    public string? NomeResponsavel { get; set; }
    public string? FormaPagamento { get; set; }
    public bool Selecionada { get; set; }
    public DateTime CriadoEm { get; set; }

    public SolicitacaoCompra SolicitacaoCompra { get; set; } = null!;
    public Fornecedor? Fornecedor { get; set; }
    public ICollection<CotacaoItem> Itens { get; set; } = [];
}
