using SindiOps.API.Constants;

namespace SindiOps.API.Entities;

public class Solicitacao
{
    public Guid Id { get; set; }
    public Guid CondominioId { get; set; }
    public Guid SolicitadoPorId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Status { get; set; } = SolicitacaoStatus.Nova;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Condominio Condominio { get; set; } = null!;
    public Usuario SolicitadoPor { get; set; } = null!;
}
