namespace SindiOps.API.Entities;

public class Sindico
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<Condominio> Condominios { get; set; } = [];
    public ICollection<Funcionario> Funcionarios { get; set; } = [];
    public ICollection<Fornecedor> Fornecedores { get; set; } = [];
    public ICollection<EmailTemplate> EmailTemplates { get; set; } = [];
    public ICollection<EmailLog> EmailLogs { get; set; } = [];
}
