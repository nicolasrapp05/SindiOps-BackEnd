namespace SindiOps.API.Constants;

public static class OcorrenciaTipoLocal
{
    public const string AreaComum = "area_comum";
    public const string Estacionamento = "estacionamento";
    public const string Portaria = "portaria";
    public const string Jardim = "jardim";
    public const string SalaoFestas = "salao_festas";
    public const string Hall = "hall";
    public const string Corredores = "corredores";
    public const string Vizinhos = "vizinhos";
    public const string Outro = "outro";

    public static readonly string[] Todos =
    [
        AreaComum, Estacionamento, Portaria, Jardim, SalaoFestas,
        Hall, Corredores, Vizinhos, Outro
    ];
}
