namespace SindiOps.API.Constants;

public static class SituacaoCotacao
{
    public const string SemCotacoes = "sem_cotacoes";
    public const string SemVencedora = "sem_vencedora";
    public const string ProntaParaAprovar = "pronta_para_aprovar";

    public static readonly string[] Todas =
    [
        SemCotacoes,
        SemVencedora,
        ProntaParaAprovar,
    ];
}
