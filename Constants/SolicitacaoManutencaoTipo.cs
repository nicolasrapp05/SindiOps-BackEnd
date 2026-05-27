namespace SindiOps.API.Constants;

/// <summary>Valores de tipo de serviço alinhados ao CHECK em <c>solicitacoes_manutencao.tipo</c>.</summary>
public static class SolicitacaoManutencaoTipo
{
    public const string ObraCivil = "obra_civil";
    public const string Pintura = "pintura";
    public const string Serralheria = "serralheria";
    public const string Eletrica = "eletrica";
    public const string Hidraulica = "hidraulica";
    public const string Cameras = "cameras";
    public const string PortasPortoes = "portas_portoes";
    public const string Jardim = "jardim";
    public const string Esgoto = "esgoto";
    public const string CaixaGordura = "caixa_gordura";
    public const string Outro = "outro";

    public static readonly string[] Todos =
    [
        ObraCivil, Pintura, Serralheria, Eletrica, Hidraulica, Cameras,
        PortasPortoes, Jardim, Esgoto, CaixaGordura, Outro
    ];
}
