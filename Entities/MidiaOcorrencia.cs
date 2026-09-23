namespace SindiOps.API.Entities;

public class MidiaOcorrencia
{
    public Guid Id { get; set; }
    public Guid OcorrenciaId { get; set; }
    public string UrlArquivo { get; set; } = string.Empty;
    public string TipoArquivo { get; set; } = string.Empty;
    public Guid EnviadoPorId { get; set; }
    public DateTime CriadoEm { get; set; }

    public Ocorrencia Ocorrencia { get; set; } = null!;
    public Usuario EnviadoPor { get; set; } = null!;
}
