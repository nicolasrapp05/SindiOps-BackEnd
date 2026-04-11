namespace SindiCore.API.Constants;

public static class RelatorioTipo
{
    public const string Ocorrencias = "ocorrencias";
    public const string MapaCotacoes = "mapa_cotacoes";
    public const string ListaCompras = "lista_compras";
    public const string AgendaPrazos = "agenda_prazos";
    public const string AgendaMandatos = "agenda_mandatos";
    public const string Manutencoes = "manutencoes";

    public static readonly string[] Todos =
    [
        Ocorrencias, MapaCotacoes, ListaCompras, AgendaPrazos, AgendaMandatos, Manutencoes
    ];
}
