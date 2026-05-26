namespace SindiCore.API.DTOs.Responses;

/// <summary>Referência slim de fornecedor usada em respostas de contratos.</summary>
public class FornecedorRefResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}
