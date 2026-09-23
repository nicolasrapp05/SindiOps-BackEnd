namespace SindiOps.API.DTOs.Responses;

public class CotacaoItemResponse
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
}

public class CotacaoResponse
{
    public Guid Id { get; set; }
    public string? NomeEmpresa { get; set; }
    public string? NomeContato { get; set; }
    public string? NomeResponsavel { get; set; }
    public string? FormaPagamento { get; set; }
    public decimal ValorTotal { get; set; }
    public bool Selecionada { get; set; }
    public FornecedorRefResponse? Fornecedor { get; set; }
    public List<CotacaoItemResponse> Itens { get; set; } = [];
}
