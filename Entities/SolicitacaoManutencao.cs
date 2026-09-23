namespace SindiOps.API.Entities;

public class SolicitacaoManutencao : Solicitacao
{
    public Guid? FornecedorId { get; set; }
    public string? Local { get; set; }
    public string TipoManutencao { get; set; } = string.Empty;
    public string? Responsavel { get; set; }
    public string? Descricao { get; set; }
    public DateOnly? DataConclusao { get; set; }

    public Fornecedor? Fornecedor { get; set; }
}
