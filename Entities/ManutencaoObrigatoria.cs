using SindiOps.API.Constants;

namespace SindiOps.API.Entities;

public class ManutencaoObrigatoria
{
    public Guid Id { get; set; }
    public Guid CondominioId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateOnly DataVencimento { get; set; }
    public DateOnly? UltimaRealizacao { get; set; }
    public string Status { get; set; } = ManutencaoStatus.Ok;
    public string? Observacoes { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Condominio Condominio { get; set; } = null!;
}
