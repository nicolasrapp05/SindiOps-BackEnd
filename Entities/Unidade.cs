namespace SindiOps.API.Entities;

public class Unidade
{
    public Guid Id { get; set; }
    public Guid BlocoId { get; set; }
    public Guid CondominioId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }

    public Bloco Bloco { get; set; } = null!;
    public Condominio Condominio { get; set; } = null!;
    public ICollection<Morador> Moradores { get; set; } = [];
}
