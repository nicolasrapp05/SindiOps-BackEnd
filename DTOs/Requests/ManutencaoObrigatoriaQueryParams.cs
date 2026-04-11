namespace SindiCore.API.DTOs.Requests;

public class ManutencaoObrigatoriaQueryParams
{
    public Guid CondominioId { get; set; }
    public string? Status { get; set; }
    public string? Tipo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
