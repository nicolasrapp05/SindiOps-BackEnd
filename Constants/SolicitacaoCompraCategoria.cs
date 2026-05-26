namespace SindiCore.API.Constants;

/// <summary>Valores de <c>categoria</c> conforme CHECK em <c>solicitacoes_compra</c>.</summary>
public static class SolicitacaoCompraCategoria
{
    public const string Papelaria = "papelaria";
    public const string MatConstrucao = "mat_construcao";
    public const string MatLimpeza = "mat_limpeza";
    public const string MatEspecifico = "mat_especifico";

    public static readonly string[] Todas =
        [Papelaria, MatConstrucao, MatLimpeza, MatEspecifico];
}
