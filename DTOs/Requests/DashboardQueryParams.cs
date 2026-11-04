namespace SindiOps.API.DTOs.Requests;

public class DashboardQueryParams
{
    public Guid? CondominioId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Sort { get; set; }
    public string? Dir { get; set; }
    public string? Tipo { get; set; }
    public string? Status { get; set; }
}
