namespace SindiOps.API.Constants;

public static class OcorrenciaTipoOcorrencia
{
    public const string Barulho = "barulho";
    public const string Pets = "pets";
    public const string Garagem = "garagem";
    public const string AlteracaoFachada = "alteracao_fachada";
    public const string ObjetosCorredores = "objetos_corredores";
    public const string ObjetosJanelasSacadas = "objetos_janelas_sacadas";
    public const string Outro = "outro";

    public static readonly string[] Todos =
    [
        Barulho, Pets, Garagem, AlteracaoFachada, ObjetosCorredores,
        ObjetosJanelasSacadas, Outro
    ];
}
