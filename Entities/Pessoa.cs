namespace SindiOps.API.Entities;

public class Pessoa
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Usuario? Usuario { get; set; }
    public ICollection<Morador> Moradores { get; set; } = [];
}
