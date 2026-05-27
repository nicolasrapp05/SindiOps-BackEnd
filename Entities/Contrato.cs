using SindiOps.API.Constants;

namespace SindiOps.API.Entities;

public class Contrato
{
    public Guid Id { get; set; }
    public Guid CondominioId { get; set; }
    public Guid FornecedorId { get; set; }
    public string TipoServico { get; set; } = string.Empty;
    public string? NomeContato { get; set; }
    public string? TelefoneContato { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public decimal? ValorMensal { get; set; }
    public string? IndiceReajuste { get; set; }
    public string? CondicoesRenovacao { get; set; }
    public string? CondicoesRescisao { get; set; }
    public string Status { get; set; } = ContratoStatus.Active;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Condominio Condominio { get; set; } = null!;
    public Fornecedor Fornecedor { get; set; } = null!;
}
