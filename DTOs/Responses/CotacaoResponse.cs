namespace SindiOps.API.DTOs.Responses;

public class CotacaoResponse
{
    public Guid Id { get; set; }
    public string? NomeEmpresa { get; set; }
    public string? NomeContato { get; set; }
    public string? NomeResponsavel { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public string? FormaPagamento { get; set; }
    public string? DescricaoProduto { get; set; }
    public decimal? Quantidade { get; set; }
    public string? Unidade { get; set; }
    public bool Selecionada { get; set; }
    public FornecedorRefResponse? Fornecedor { get; set; }
}
