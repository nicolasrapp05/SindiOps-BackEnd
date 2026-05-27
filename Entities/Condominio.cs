namespace SindiOps.API.Entities;

public class Condominio
{
    public Guid Id { get; set; }
    public Guid SindicoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? EnderecoRua { get; set; }
    public string? EnderecoNumero { get; set; }
    public string? EnderecoBairro { get; set; }
    public string? EnderecoCidade { get; set; }
    public string? EnderecoCep { get; set; }
    public DateOnly? DataEleicao { get; set; }
    public DateOnly? VencimentoMandato { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Sindico Sindico { get; set; } = null!;
    public ICollection<Bloco> Blocos { get; set; } = [];
    public ICollection<Unidade> Unidades { get; set; } = [];
    public ICollection<Morador> Moradores { get; set; } = [];
    public ICollection<Contrato> Contratos { get; set; } = [];
    public ICollection<ManutencaoObrigatoria> ManutencoesObrigatorias { get; set; } = [];
    public ICollection<SolicitacaoManutencao> SolicitacoesManutencao { get; set; } = [];
    public ICollection<SolicitacaoCompra> SolicitacoesCompra { get; set; } = [];
    public ICollection<Ocorrencia> Ocorrencias { get; set; } = [];
}
