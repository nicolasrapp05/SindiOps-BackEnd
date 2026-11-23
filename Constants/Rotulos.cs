namespace SindiOps.API.Constants;

/// <summary>
/// Textos exibidos ao usuário. Os códigos persistidos não mudam.
/// Rótulos alinhados aos mapas do frontend. Código desconhecido permanece o próprio código.
/// </summary>
public static class Rotulos
{
    private static readonly Dictionary<string, string> OcorrenciaTipoMap = new(StringComparer.Ordinal)
    {
        [OcorrenciaTipoOcorrencia.Barulho] = "Barulho",
        [OcorrenciaTipoOcorrencia.Pets] = "Pets",
        [OcorrenciaTipoOcorrencia.Garagem] = "Garagem",
        [OcorrenciaTipoOcorrencia.AlteracaoFachada] = "Fachada",
        [OcorrenciaTipoOcorrencia.ObjetosCorredores] = "Objetos Corredores",
        [OcorrenciaTipoOcorrencia.ObjetosJanelasSacadas] = "Objetos Janelas/Sacadas",
        [OcorrenciaTipoOcorrencia.Outro] = "Outro",
    };

    private static readonly Dictionary<string, string> FluxoStatusMap = new(StringComparer.Ordinal)
    {
        [OcorrenciaStatus.Nova] = "Nova",
        [OcorrenciaStatus.EmAndamento] = "Em Andamento",
        [OcorrenciaStatus.Finalizada] = "Finalizada",
        [OcorrenciaStatus.Cancelada] = "Cancelada",
    };

    private static readonly Dictionary<string, string> OcorrenciaOrigemMap = new(StringComparer.Ordinal)
    {
        [OcorrenciaOrigem.ReclamacaoMorador] = "Morador",
        [OcorrenciaOrigem.ReclamacaoFuncionario] = "Funcionário",
        [OcorrenciaOrigem.ReclamacaoTerceiros] = "Terceiros",
        [OcorrenciaOrigem.ForaDeNorma] = "Fora de norma",
    };

    private static readonly Dictionary<string, string> ManutencaoTipoMap = new(StringComparer.Ordinal)
    {
        [ManutencaoObrigatoriaTipo.Dedetizacao] = "Dedetização",
        [ManutencaoObrigatoriaTipo.ParaRaios] = "Para-raios",
        [ManutencaoObrigatoriaTipo.Seguro] = "Seguro",
        [ManutencaoObrigatoriaTipo.LimpezaCaixaAgua] = "Limpeza Caixa d\u2019Água",
        [ManutencaoObrigatoriaTipo.CaixaGorduraEsgoto] = "Caixa Gordura/Esgoto",
        [ManutencaoObrigatoriaTipo.Extintores] = "Extintores",
        [ManutencaoObrigatoriaTipo.Cvcb] = "CVCB (Bombeiros)",
        [ManutencaoObrigatoriaTipo.CalhasTelhado] = "Calhas e Telhado",
        [ManutencaoObrigatoriaTipo.Ppra] = "PPRA",
        [ManutencaoObrigatoriaTipo.Pcmso] = "PCMSO",
        [ManutencaoObrigatoriaTipo.Pgr] = "PGR",
    };

    private static readonly Dictionary<string, string> ManutencaoStatusMap = new(StringComparer.Ordinal)
    {
        [ManutencaoStatus.Ok] = "OK",
        [ManutencaoStatus.Upcoming] = "Próxima",
        [ManutencaoStatus.Overdue] = "Vencida",
    };

    private static readonly Dictionary<string, string> ContratoTipoMap = new(StringComparer.Ordinal)
    {
        ["administradora"] = "Administradora",
        ["garantidora"] = "Garantidora",
        ["gas"] = "Gás",
        ["telefonia"] = "Telefonia",
        ["internet"] = "Internet",
        ["terceirizada"] = "Terceirizada",
        ["juridico"] = "Jurídico",
        ["manutencao_elevador"] = "Manutenção Elevador",
        ["manutencao_jardim"] = "Manutenção Jardim",
        ["gestao_residuos"] = "Gestão de Resíduos",
        ["outro"] = "Outro",
    };

    private static readonly Dictionary<string, string> ContratoStatusMap = new(StringComparer.Ordinal)
    {
        [ContratoStatus.Active] = "Vigente",
        [ContratoStatus.Expiring] = "Expirando",
        [ContratoStatus.Expired] = "Expirado",
        [ContratoStatus.Cancelled] = "Cancelado",
    };

    private static readonly Dictionary<string, string> CompraCategoriaMap = new(StringComparer.Ordinal)
    {
        [SolicitacaoCompraCategoria.Papelaria] = "Papelaria",
        [SolicitacaoCompraCategoria.MatConstrucao] = "Material de Construção",
        [SolicitacaoCompraCategoria.MatLimpeza] = "Material de Limpeza",
        [SolicitacaoCompraCategoria.MatEspecifico] = "Material Específico",
    };

    private static readonly Dictionary<string, string> TemplateTipoMap = new(StringComparer.Ordinal)
    {
        [EmailTemplateTipo.Advertencia] = "Advertência",
        [EmailTemplateTipo.Multa] = "Multa",
        [EmailTemplateTipo.NotificacaoOcorrencia] = "Notificação de Ocorrência",
        [EmailTemplateTipo.ComunicadoGeral] = "Comunicado Geral",
        [EmailTemplateTipo.NotificacaoManutencao] = "Notificação de Manutenção",
    };

    public static string OcorrenciaTipo(string? codigo) => Texto(codigo, OcorrenciaTipoMap);

    public static string StatusOcorrencia(string? codigo) => Texto(codigo, FluxoStatusMap);

    public static string OrigemOcorrencia(string? codigo) => Texto(codigo, OcorrenciaOrigemMap);

    public static string TipoManutencaoObrigatoria(string? codigo) => Texto(codigo, ManutencaoTipoMap);

    public static string StatusManutencao(string? codigo) => Texto(codigo, ManutencaoStatusMap);

    public static string ContratoTipoServico(string? codigo) => Texto(codigo, ContratoTipoMap);

    public static string StatusContrato(string? codigo) => Texto(codigo, ContratoStatusMap);

    public static string CompraCategoria(string? codigo) => Texto(codigo, CompraCategoriaMap);

    public static string CompraStatus(string? codigo) => Texto(codigo, FluxoStatusMap);

    public static string TemplateTipo(string? codigo) => Texto(codigo, TemplateTipoMap);

    private static string Texto(string? codigo, Dictionary<string, string> mapa)
    {
        if (string.IsNullOrEmpty(codigo))
            return codigo ?? string.Empty;
        return mapa.TryGetValue(codigo, out var rotulo) ? rotulo : codigo;
    }
}
