namespace SindiOps.API.Entities;

/// <summary>
/// Ocupação de uma unidade. O <see cref="Id"/> é a chave que a API já usava
/// para cada vínculo morador-unidade e é preservado na migração.
/// </summary>
public class MoradorUnidade
{
    public Guid Id { get; set; }
    public Guid MoradorId { get; set; }
    public Guid UnidadeId { get; set; }
    public string Papel { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
    public DateTime? DeletadoEm { get; set; }

    public Morador Morador { get; set; } = null!;
    public Unidade Unidade { get; set; } = null!;
}
