using SindiCore.API.Constants;

namespace SindiCore.API.Entities;

public class Ocorrencia
{
    public Guid Id { get; set; }
    public Guid CondominioId { get; set; }
    public Guid? RegistradoPorFuncionarioId { get; set; }
    public Guid? RegistradoPorSindicoId { get; set; }
    public Guid? MoradorId { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string TipoLocal { get; set; } = string.Empty;
    public Guid? BlocoId { get; set; }
    public Guid? UnidadeId { get; set; }
    public string TipoOcorrencia { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime OcorreuEm { get; set; }
    public string Status { get; set; } = OcorrenciaStatus.Nova;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }

    public Condominio Condominio { get; set; } = null!;
    public Funcionario? RegistradoPorFuncionario { get; set; }
    public Sindico? RegistradoPorSindico { get; set; }
    public Morador? Morador { get; set; }
    public Bloco? Bloco { get; set; }
    public Unidade? Unidade { get; set; }
    public ICollection<MidiaOcorrencia> Midias { get; set; } = [];
    public ICollection<EmailLog> EmailLogs { get; set; } = [];
}
