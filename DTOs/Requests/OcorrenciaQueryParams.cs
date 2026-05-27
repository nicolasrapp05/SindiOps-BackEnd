namespace SindiOps.API.DTOs.Requests;

public class OcorrenciaQueryParams
{
    public Guid CondominioId { get; set; }
    public string? Status { get; set; }
    public string? Origem { get; set; }
    public string? TipoOcorrencia { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
