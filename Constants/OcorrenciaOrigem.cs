namespace SindiOps.API.Constants;

public static class OcorrenciaOrigem
{
    public const string ReclamacaoMorador = "reclamacao_morador";
    public const string ReclamacaoFuncionario = "reclamacao_funcionario";
    public const string ReclamacaoTerceiros = "reclamacao_terceiros";
    public const string ForaDeNorma = "fora_de_norma";

    public static readonly string[] Todas =
        [ReclamacaoMorador, ReclamacaoFuncionario, ReclamacaoTerceiros, ForaDeNorma];
}
