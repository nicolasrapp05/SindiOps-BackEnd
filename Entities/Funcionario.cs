namespace SindiOps.API.Entities;

public class Funcionario
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid SindicoId { get; set; }
    public string Cargo { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Sindico Sindico { get; set; } = null!;
    public ICollection<FuncionarioCondominio> CondominiosAcesso { get; set; } = [];
}
