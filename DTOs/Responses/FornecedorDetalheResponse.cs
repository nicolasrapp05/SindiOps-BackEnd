namespace SindiOps.API.DTOs.Responses;

public class FornecedorDetalheResponse : FornecedorResponse
{
    public string? EnderecoRua { get; set; }
    public string? EnderecoNumero { get; set; }
    public string? EnderecoBairro { get; set; }
    public string? EnderecoCidade { get; set; }
    public string? EnderecoCep { get; set; }
    public string? Instagram { get; set; }
    public string? Website { get; set; }
}
