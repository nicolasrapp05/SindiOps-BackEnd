namespace SindiCore.API.DTOs.Requests;

public class CreateOcorrenciaRequest
{
    public Guid CondominioId { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string TipoLocal { get; set; } = string.Empty;
    public string TipoOcorrencia { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime OcorreuEm { get; set; }
    public Guid? MoradorId { get; set; }
    public Guid? BlocoId { get; set; }
    public Guid? UnidadeId { get; set; }
}
