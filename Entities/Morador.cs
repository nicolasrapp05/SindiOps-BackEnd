namespace SindiOps.API.Entities;

public class Morador
{
    public Guid Id { get; set; }
    public Guid PessoaId { get; set; }
    public Guid UnidadeId { get; set; }
    public string Papel { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public DateTime? DeletadoEm { get; set; }

    public Pessoa Pessoa { get; set; } = null!;
    public Unidade Unidade { get; set; } = null!;
    public ICollection<EmailLog> EmailLogs { get; set; } = [];
    public ICollection<Ocorrencia> Ocorrencias { get; set; } = [];
}
