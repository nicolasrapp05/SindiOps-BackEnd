namespace SindiOps.API.Entities;

public class FuncionarioCondominio
{
    public Guid FuncionarioId { get; set; }
    public Guid CondominioId { get; set; }
    public DateTime CriadoEm { get; set; }

    public Funcionario Funcionario { get; set; } = null!;
    public Condominio Condominio { get; set; } = null!;
}
