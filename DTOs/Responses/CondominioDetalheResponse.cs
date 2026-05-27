namespace SindiOps.API.DTOs.Responses;

public class CondominioDetalheResponse : CondominioResponse
{
    public List<BlocoResponse> Blocos { get; set; } = [];
}
