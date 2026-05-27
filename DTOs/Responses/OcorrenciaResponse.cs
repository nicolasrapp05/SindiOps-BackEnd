namespace SindiOps.API.DTOs.Responses;

public class OcorrenciaResponse
{
    public Guid Id { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string TipoLocal { get; set; } = string.Empty;
    public string TipoOcorrencia { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime OcorreuEm { get; set; }
    public MoradorOcorrenciaRefResponse? Morador { get; set; }
    public BlocoNomeRefResponse? Bloco { get; set; }
    public UnidadeNumeroRefResponse? Unidade { get; set; }
    public PessoaRefResponse RegistradoPor { get; set; } = null!;
    public int TotalMidias { get; set; }
    public DateTime CriadoEm { get; set; }
}
