namespace SindiOps.API.DTOs.Requests;

public class SolicitacaoCompraQueryParams
{
    public Guid CondominioId { get; set; }
    public string? Status { get; set; }
    public string? Categoria { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
