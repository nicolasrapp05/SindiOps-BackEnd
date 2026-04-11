namespace SindiCore.API.Entities;

public class Bloco
{
    public Guid Id { get; set; }
    public Guid CondominioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }

    public Condominio Condominio { get; set; } = null!;
    public ICollection<Unidade> Unidades { get; set; } = [];
    public ICollection<Morador> Moradores { get; set; } = [];
}
