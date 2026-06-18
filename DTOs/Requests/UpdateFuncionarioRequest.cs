namespace SindiOps.API.DTOs.Requests;

public class UpdateFuncionarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public List<Guid> CondominioIds { get; set; } = [];
}
