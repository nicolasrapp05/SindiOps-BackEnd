namespace SindiOps.API.Entities;

public class Morador
{
    public Guid Id { get; set; }
    public Guid CondominioId { get; set; }
    public Guid BlocoId { get; set; }
    public Guid UnidadeId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public DateTime? DeletadoEm { get; set; }

    public Condominio Condominio { get; set; } = null!;
    public Bloco Bloco { get; set; } = null!;
    public Unidade Unidade { get; set; } = null!;
    public ICollection<EmailLog> EmailLogs { get; set; } = [];
    public ICollection<Ocorrencia> Ocorrencias { get; set; } = [];
}
