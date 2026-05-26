namespace SindiCore.API.Entities;

public class ServicoFornecedor
{
    public Guid Id { get; set; }
    public Guid FornecedorId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal? Quantidade { get; set; }
    public DateTime CriadoEm { get; set; }

    public Fornecedor Fornecedor { get; set; } = null!;
}
