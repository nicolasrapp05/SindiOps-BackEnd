namespace SindiOps.API.DTOs.Responses;

public class SolicitacaoCompraResponse
{
    public Guid Id { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Item { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public bool EReposicao { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TipoAprovacao { get; set; }
    public PessoaRefResponse? AprovadoPor { get; set; }
    public PessoaRefResponse SolicitadoPor { get; set; } = null!;
    public int TotalCotacoes { get; set; }
    public DateTime CriadoEm { get; set; }
}
