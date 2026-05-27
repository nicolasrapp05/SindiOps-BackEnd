namespace SindiOps.API.DTOs.Responses;

public class MoradorDetalheResponse : MoradorResponse
{
    public List<EmailLogResumoResponse> UltimosEmails { get; set; } = [];
}
