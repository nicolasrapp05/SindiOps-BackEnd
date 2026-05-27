namespace SindiOps.API.DTOs.Requests;

public class UpdateSolicitacaoStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public DateOnly? DataConclusao { get; set; }
}
