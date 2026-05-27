using SindiOps.API.Constants;

namespace SindiOps.API.Entities;

public class SolicitacaoCompra
{
    public Guid Id { get; set; }
    public Guid CondominioId { get; set; }
    public Guid? SolicitadoPorFuncionarioId { get; set; }
    public Guid? SolicitadoPorSindicoId { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Item { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public bool EReposicao { get; set; } = false;
    public string? Justificativa { get; set; }
    public string? TipoAprovacao { get; set; }
    public Guid? AprovadoPorId { get; set; }
    public string Status { get; set; } = SolicitacaoStatus.Nova;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Condominio Condominio { get; set; } = null!;
    public Funcionario? SolicitadoPorFuncionario { get; set; }
    public Sindico? SolicitadoPorSindico { get; set; }
    public Funcionario? AprovadoPor { get; set; }
    public ICollection<Cotacao> Cotacoes { get; set; } = [];
}
