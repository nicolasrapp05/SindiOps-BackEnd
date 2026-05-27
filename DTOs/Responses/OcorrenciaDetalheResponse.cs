namespace SindiOps.API.DTOs.Responses;

public class OcorrenciaDetalheResponse : OcorrenciaResponse
{
    public Guid CondominioId { get; set; }
    public List<MidiaResponse> Midias { get; set; } = [];
    public List<EmailLogResumoResponse> EmailLogs { get; set; } = [];
}
