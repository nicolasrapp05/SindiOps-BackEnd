namespace SindiOps.API.Entities;

public class OrcamentoManutencao
{
    public Guid Id { get; set; }
    public Guid SolicitacaoManutencaoId { get; set; }
    public Guid? FornecedorId { get; set; }
    public string? NomeEmpresa { get; set; }
    public string? NomeContato { get; set; }
    public string? NomeResponsavel { get; set; }
    public string? FormaPagamento { get; set; }
    public string? DescricaoEscopo { get; set; }
    public decimal Valor { get; set; }
    public int? PrazoExecucaoDias { get; set; }
    public DateOnly? ValidadeProposta { get; set; }
    public string? Garantia { get; set; }
    public string? Observacoes { get; set; }
    public bool Selecionada { get; set; }
    public DateTime CriadoEm { get; set; }

    public SolicitacaoManutencao SolicitacaoManutencao { get; set; } = null!;
    public Fornecedor? Fornecedor { get; set; }
}
