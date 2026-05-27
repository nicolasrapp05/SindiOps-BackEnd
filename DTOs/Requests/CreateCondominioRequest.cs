namespace SindiOps.API.DTOs.Requests;

public class CreateCondominioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? EnderecoRua { get; set; }
    public string? EnderecoNumero { get; set; }
    public string? EnderecoBairro { get; set; }
    public string? EnderecoCidade { get; set; }
    public string? EnderecoCep { get; set; }
    public DateOnly? DataEleicao { get; set; }
    public DateOnly? VencimentoMandato { get; set; }
}
