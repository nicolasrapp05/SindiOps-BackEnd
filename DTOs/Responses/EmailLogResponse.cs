namespace SindiOps.API.DTOs.Responses;

public class EmailLogResponse
{
    public Guid Id { get; set; }
    public string Assunto { get; set; } = string.Empty;
    public string EmailDestinatario { get; set; } = string.Empty;
    public MoradorEmailLogRefResponse Morador { get; set; } = null!;
    public OcorrenciaEmailLogRefResponse? Ocorrencia { get; set; }
    public TemplateEmailLogRefResponse? Template { get; set; }
    public string StatusEntrega { get; set; } = string.Empty;
    public DateTime EnviadoEm { get; set; }
    public PessoaRefResponse EnviadoPor { get; set; } = null!;
}
