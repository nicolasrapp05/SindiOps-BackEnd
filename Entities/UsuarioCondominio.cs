namespace SindiOps.API.Entities;

public class UsuarioCondominio
{
    public Guid UsuarioId { get; set; }
    public Guid CondominioId { get; set; }
    public DateTime CriadoEm { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Condominio Condominio { get; set; } = null!;
}
