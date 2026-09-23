namespace SindiOps.API.DTOs.Requests;

public class SolicitacaoCompraItemRequest
{
    public Guid? Id { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public string? Unidade { get; set; }
    public bool EReposicao { get; set; }
}

public class CreateSolicitacaoCompraRequest
{
    public Guid CondominioId { get; set; }
    public string? Justificativa { get; set; }
    public string TipoAprovacao { get; set; } = string.Empty;
    public List<SolicitacaoCompraItemRequest> Itens { get; set; } = [];
}
