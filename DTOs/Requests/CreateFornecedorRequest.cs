namespace SindiOps.API.DTOs.Requests;

public class CreateFornecedorRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? EnderecoRua { get; set; }
    public string? EnderecoNumero { get; set; }
    public string? EnderecoBairro { get; set; }
    public string? EnderecoCidade { get; set; }
    public string? EnderecoCep { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Instagram { get; set; }
    public string? Website { get; set; }
    public string? NomeContato { get; set; }
    public List<CreateServicoRequest> Servicos { get; set; } = [];
}
