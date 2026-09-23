namespace SindiOps.API.Entities;

public class Usuario
{
    public Guid Id { get; set; }
    public Guid PessoaId { get; set; }
    public Guid? SindicoId { get; set; }
    public string Cargo { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Pessoa Pessoa { get; set; } = null!;
    public Usuario? Sindico { get; set; }
    public ICollection<Usuario> Equipe { get; set; } = [];
    public ICollection<UsuarioCondominio> CondominiosAcesso { get; set; } = [];
    public ICollection<Condominio> Condominios { get; set; } = [];
    public ICollection<Fornecedor> Fornecedores { get; set; } = [];
    public ICollection<EmailTemplate> EmailTemplates { get; set; } = [];
    public ICollection<EmailLog> EmailLogs { get; set; } = [];
}
