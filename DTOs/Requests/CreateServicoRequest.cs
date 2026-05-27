namespace SindiOps.API.DTOs.Requests;

public class CreateServicoRequest
{
    public string Tipo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal? Quantidade { get; set; }
}
