using SindiOps.API.Constants;

namespace SindiOps.API.Entities;

public class EmailLog
{
    public Guid Id { get; set; }
    public Guid SindicoId { get; set; }
    public Guid? TemplateId { get; set; }
    public Guid? OcorrenciaId { get; set; }
    public Guid MoradorId { get; set; }
    public string EmailDestinatario { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public string CorpoResolvido { get; set; } = string.Empty;
    public decimal? ValorMulta { get; set; }
    public Guid? EnviadoPorFuncionarioId { get; set; }
    public Guid? EnviadoPorSindicoId { get; set; }
    public DateTime EnviadoEm { get; set; }
    public string StatusEntrega { get; set; } = EmailLogStatus.Sent;
    public DateTime CriadoEm { get; set; }

    public Sindico Sindico { get; set; } = null!;
    public EmailTemplate? Template { get; set; }
    public Ocorrencia? Ocorrencia { get; set; }
    public Morador Morador { get; set; } = null!;
    public Funcionario? EnviadoPorFuncionario { get; set; }
    public Sindico? EnviadoPorSindico { get; set; }
}
