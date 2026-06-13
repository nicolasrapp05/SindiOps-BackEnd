namespace SindiOps.API.DTOs.Requests;

public class ContratoQueryParams
{
    public Guid? CondominioId { get; set; }
    public Guid? FornecedorId { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
