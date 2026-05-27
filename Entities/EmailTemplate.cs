namespace SindiOps.API.Entities;

public class EmailTemplate
{
    public Guid Id { get; set; }
    public Guid SindicoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public string Corpo { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Sindico Sindico { get; set; } = null!;
    public ICollection<EmailLog> EmailLogs { get; set; } = [];
}
