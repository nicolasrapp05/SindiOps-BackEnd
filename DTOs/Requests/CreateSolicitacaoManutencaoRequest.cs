namespace SindiOps.API.DTOs.Requests;

public class CreateSolicitacaoManutencaoRequest
{
    public Guid CondominioId { get; set; }
    public string TipoServico { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Guid? FornecedorId { get; set; }
}
