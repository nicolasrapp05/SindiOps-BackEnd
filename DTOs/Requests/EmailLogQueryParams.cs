namespace SindiOps.API.DTOs.Requests;

public class EmailLogQueryParams
{
    public Guid? CondominioId { get; set; }
    public Guid? MoradorId { get; set; }
    public Guid? OcorrenciaId { get; set; }
    public string? StatusEntrega { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
