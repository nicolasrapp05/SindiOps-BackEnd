using System.Text.RegularExpressions;

namespace SindiOps.API.Helpers;

/// <summary>
/// O tipo do serviço do fornecedor é texto livre.
/// Só vira título quando foi gravado inteiro em snake_case, como o placeholder antigo sugeria.
/// </summary>
public static class ServicoTipoTexto
{
    private static readonly Regex SnakeCasePuro = new(
        "^[a-z0-9]+(_[a-z0-9]+)+$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static string Normalizar(string tipo)
    {
        var texto = tipo.Trim();
        if (!SnakeCasePuro.IsMatch(texto))
            return texto;

        return string.Join(' ', texto.Split('_', StringSplitOptions.RemoveEmptyEntries).Select(Capitalizar));
    }

    private static string Capitalizar(string palavra) =>
        char.ToUpperInvariant(palavra[0]) + palavra[1..];
}
