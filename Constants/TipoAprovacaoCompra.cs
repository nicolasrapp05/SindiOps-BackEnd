namespace SindiOps.API.Constants;

public static class TipoAprovacaoCompra
{
    public const string Sindico = "sindico";
    public const string Conselho = "conselho";
    public const string Assembleia = "assembleia";

    public static readonly string[] Todos = [Sindico, Conselho, Assembleia];
}
