namespace SindiOps.API.Entities;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Sindico? Sindico { get; set; }
    public Funcionario? Funcionario { get; set; }
    public Morador? Morador { get; set; }
}
