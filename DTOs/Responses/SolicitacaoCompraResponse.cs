namespace SindiOps.API.DTOs.Responses;

public class SolicitacaoCompraItemResponse
{
    public Guid Id { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public string? Unidade { get; set; }
    public bool EReposicao { get; set; }
}

public class SolicitacaoCompraResponse
{
    public Guid Id { get; set; }
    public List<SolicitacaoCompraItemResponse> Itens { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public string? TipoAprovacao { get; set; }
    public PessoaRefResponse? AprovadoPor { get; set; }
    public PessoaRefResponse SolicitadoPor { get; set; } = null!;
    public int TotalCotacoes { get; set; }
    public bool TemCotacaoSelecionada { get; set; }
    public DateTime CriadoEm { get; set; }
}
