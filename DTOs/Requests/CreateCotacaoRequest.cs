namespace SindiOps.API.DTOs.Requests;

public class CotacaoItemRequest
{
    public Guid ItemId { get; set; }
    public decimal ValorUnitario { get; set; }
}

public class CreateCotacaoRequest
{
    public Guid? FornecedorId { get; set; }
    public string? NomeEmpresa { get; set; }
    public string? NomeContato { get; set; }
    public string? NomeResponsavel { get; set; }
    public string? FormaPagamento { get; set; }
    public List<CotacaoItemRequest> Itens { get; set; } = [];
}
