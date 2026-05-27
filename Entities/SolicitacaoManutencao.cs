using SindiOps.API.Constants;

namespace SindiOps.API.Entities;

public class SolicitacaoManutencao
{
    public Guid Id { get; set; }
    public Guid CondominioId { get; set; }
    public Guid? SolicitadoPorFuncionarioId { get; set; }
    public Guid? SolicitadoPorSindicoId { get; set; }
    public Guid? FornecedorId { get; set; }
    public string? Local { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? Responsavel { get; set; }
    public string? Descricao { get; set; }
    public string Status { get; set; } = SolicitacaoStatus.Nova;
    public DateOnly? DataConclusao { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Condominio Condominio { get; set; } = null!;
    public Funcionario? SolicitadoPorFuncionario { get; set; }
    public Sindico? SolicitadoPorSindico { get; set; }
    public Fornecedor? Fornecedor { get; set; }
}
