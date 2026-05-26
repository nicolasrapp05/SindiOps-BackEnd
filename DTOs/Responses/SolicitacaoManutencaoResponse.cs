namespace SindiCore.API.DTOs.Responses;

public class SolicitacaoManutencaoResponse
{
    public Guid Id { get; set; }
    public string TipoServico { get; set; } = string.Empty;
    public string? Local { get; set; }
    public string? Responsavel { get; set; }
    public string Status { get; set; } = string.Empty;
    public FornecedorRefResponse? Fornecedor { get; set; }
    public DateOnly? DataConclusao { get; set; }
    public PessoaRefResponse RegistradoPor { get; set; } = null!;
    public DateTime CriadoEm { get; set; }
    public string? Descricao { get; set; }
}
