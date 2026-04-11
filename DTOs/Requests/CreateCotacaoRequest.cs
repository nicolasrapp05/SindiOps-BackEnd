namespace SindiCore.API.DTOs.Requests;

public class CreateCotacaoRequest
{
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
}
