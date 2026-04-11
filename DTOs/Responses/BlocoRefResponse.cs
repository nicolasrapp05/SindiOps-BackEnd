namespace SindiCore.API.DTOs.Responses;

/// <summary>Referência simplificada de bloco (sem lista de unidades) usada em respostas aninhadas.</summary>
public class BlocoRefResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}
