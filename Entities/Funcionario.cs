namespace SindiOps.API.Entities;

public class Funcionario
{
    public Guid Id { get; set; }
    public Guid SindicoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Sindico Sindico { get; set; } = null!;
    public ICollection<FuncionarioCondominio> CondominiosAcesso { get; set; } = new List<FuncionarioCondominio>();
}
