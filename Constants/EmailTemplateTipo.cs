namespace SindiCore.API.Constants;

public static class EmailTemplateTipo
{
    public const string Advertencia = "advertencia";
    public const string Multa = "multa";
    public const string NotificacaoOcorrencia = "notificacao_ocorrencia";
    public const string ComunicadoGeral = "comunicado_geral";
    public const string NotificacaoManutencao = "notificacao_manutencao";

    public static readonly string[] Todos =
    [
        Advertencia, Multa, NotificacaoOcorrencia, ComunicadoGeral, NotificacaoManutencao
    ];
}
