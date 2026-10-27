namespace SindiOps.API.Constants;

public static class SituacaoOrcamento
{
    public const string SemOrcamento = "sem_orcamento";
    public const string AguardandoProposta = "aguardando_proposta";
    public const string PropostaSelecionada = "proposta_selecionada";
    public const string Aprovado = "aprovado";
    public const string Encerrado = "encerrado";

    public static readonly string[] Todas =
    [
        SemOrcamento,
        AguardandoProposta,
        PropostaSelecionada,
        Aprovado,
        Encerrado,
    ];

    public static string Calcular(
        string status,
        bool temOrcamentos,
        bool temSelecionada,
        bool aprovado)
    {
        var fechado = status is SolicitacaoStatus.Finalizada or SolicitacaoStatus.Cancelada;
        if (fechado && temOrcamentos)
            return Encerrado;
        if (!fechado && aprovado)
            return Aprovado;
        if (!fechado && temSelecionada)
            return PropostaSelecionada;
        if (!fechado && temOrcamentos)
            return AguardandoProposta;
        return SemOrcamento;
    }
}
