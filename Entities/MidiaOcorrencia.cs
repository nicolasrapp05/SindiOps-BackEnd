namespace SindiOps.API.Entities;

public class MidiaOcorrencia
{
    public Guid Id { get; set; }
    public Guid OcorrenciaId { get; set; }
    public string UrlArquivo { get; set; } = string.Empty;
    public string TipoArquivo { get; set; } = string.Empty;
    public Guid? EnviadoPorFuncionarioId { get; set; }
    public Guid? EnviadoPorSindicoId { get; set; }
    public DateTime CriadoEm { get; set; }

    public Ocorrencia Ocorrencia { get; set; } = null!;
    public Funcionario? EnviadoPorFuncionario { get; set; }
    public Sindico? EnviadoPorSindico { get; set; }
}
