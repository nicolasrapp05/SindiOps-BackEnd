namespace SindiOps.API.Entities;

public class Morador
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public ICollection<MoradorUnidade> Unidades { get; set; } = [];
    public ICollection<EmailLog> EmailLogs { get; set; } = [];
    public ICollection<Ocorrencia> Ocorrencias { get; set; } = [];
}
