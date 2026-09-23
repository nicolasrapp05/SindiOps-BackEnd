using SindiOps.API.Constants;

namespace SindiOps.API.Entities;

public class SolicitacaoCompra : Solicitacao
{
    public string? Justificativa { get; set; }
    public string? TipoAprovacao { get; set; }
    public Guid? AprovadoPorId { get; set; }

    public Usuario? AprovadoPor { get; set; }
    public ICollection<SolicitacaoCompraItem> Itens { get; set; } = [];
    public ICollection<Cotacao> Cotacoes { get; set; } = [];
}
