namespace SindiCore.API.Constants;

/// <summary>Valores de <c>tipo</c> alinhados ao CHECK em <c>manutencoes_obrigatorias</c> (schema).</summary>
public static class ManutencaoObrigatoriaTipo
{
    public const string Dedetizacao = "dedetizacao";
    public const string ParaRaios = "para_raios";
    public const string Seguro = "seguro";
    public const string LimpezaCaixaAgua = "limpeza_caixa_agua";
    public const string CaixaGorduraEsgoto = "caixa_gordura_esgoto";
    public const string Extintores = "extintores";
    public const string Cvcb = "cvcb";
    public const string CalhasTelhado = "calhas_telhado";
    public const string Ppra = "ppra";
    public const string Pcmso = "pcmso";
    public const string Pgr = "pgr";

    public static readonly string[] Todos =
    [
        Dedetizacao, ParaRaios, Seguro, LimpezaCaixaAgua, CaixaGorduraEsgoto,
        Extintores, Cvcb, CalhasTelhado, Ppra, Pcmso, Pgr
    ];

    /// <summary>Meses a somar à data de realização para obter a próxima data de vencimento.</summary>
    public static int GetMesesAposRealizacao(string tipo) => tipo switch
    {
        Dedetizacao => 6,
        ParaRaios => 12,
        Seguro => 12,
        LimpezaCaixaAgua => 6,
        CaixaGorduraEsgoto => 6,
        Extintores => 12,
        Cvcb => 12,
        CalhasTelhado => 24,
        Ppra => 12,
        Pcmso => 12,
        Pgr => 12,
        _ => 12
    };
}
