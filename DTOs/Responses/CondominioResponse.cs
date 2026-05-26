namespace SindiCore.API.DTOs.Responses;

public class CondominioResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? EnderecoRua { get; set; }
    public string? EnderecoNumero { get; set; }
    public string? EnderecoBairro { get; set; }
    public string? EnderecoCidade { get; set; }
    public string? EnderecoCep { get; set; }
    public DateOnly? DataEleicao { get; set; }
    public DateOnly? VencimentoMandato { get; set; }
    public int TotalBlocos { get; set; }
    public int TotalUnidades { get; set; }
    public DateTime CriadoEm { get; set; }
}
