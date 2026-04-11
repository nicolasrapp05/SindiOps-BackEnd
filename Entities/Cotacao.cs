namespace SindiCore.API.Entities;

public class Cotacao
{
    public Guid Id { get; set; }
    public Guid SolicitacaoCompraId { get; set; }
    public Guid? FornecedorId { get; set; }
    public string? NomeEmpresa { get; set; }
    public string? NomeContato { get; set; }
    public string? NomeResponsavel { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public string? FormaPagamento { get; set; }
    public string? DescricaoProduto { get; set; }
    public decimal? Quantidade { get; set; }
    public string? Unidade { get; set; }
    public bool Selecionada { get; set; } = false;
    public DateTime CriadoEm { get; set; }

    public SolicitacaoCompra SolicitacaoCompra { get; set; } = null!;
    public Fornecedor? Fornecedor { get; set; }
}
