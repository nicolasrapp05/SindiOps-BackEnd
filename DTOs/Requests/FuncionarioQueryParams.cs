namespace SindiOps.API.DTOs.Requests;

public class FuncionarioQueryParams
{
    public string? Cargo { get; set; }
    public bool? Ativo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Sort { get; set; }
    public string? Dir { get; set; }
}
