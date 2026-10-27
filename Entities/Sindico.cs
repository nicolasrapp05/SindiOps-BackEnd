namespace SindiOps.API.Entities;

public class Sindico
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public ICollection<Funcionario> Equipe { get; set; } = [];
    public ICollection<Condominio> Condominios { get; set; } = [];
    public ICollection<Fornecedor> Fornecedores { get; set; } = [];
    public ICollection<EmailTemplate> EmailTemplates { get; set; } = [];
    public ICollection<EmailLog> EmailLogs { get; set; } = [];
}
