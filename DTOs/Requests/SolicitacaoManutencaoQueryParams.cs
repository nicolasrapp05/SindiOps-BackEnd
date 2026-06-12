namespace SindiOps.API.DTOs.Requests;

public class SolicitacaoManutencaoQueryParams
{
    public Guid CondominioId { get; set; }
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? TipoServico { get; set; }
    public string? Responsavel { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
