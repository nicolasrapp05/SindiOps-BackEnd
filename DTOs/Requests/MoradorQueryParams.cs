namespace SindiCore.API.DTOs.Requests;

public class MoradorQueryParams
{
    public Guid? BlocoId { get; set; }
    public Guid? UnidadeId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
