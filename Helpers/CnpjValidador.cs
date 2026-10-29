using System.Text.RegularExpressions;

namespace SindiOps.API.Helpers;

public static class CnpjValidador
{
    public static bool EhValido(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return true;

        var digitos = Regex.Replace(cnpj, @"\D", "");
        if (digitos.Length != 14 || digitos.Distinct().Count() == 1)
            return false;

        var numeros = digitos.Select(c => c - '0').ToArray();
        return Digito(numeros, 12) == numeros[12] && Digito(numeros, 13) == numeros[13];
    }

    private static int Digito(int[] numeros, int tamanho)
    {
        var soma = 0;
        var peso = tamanho - 7;
        for (var i = 0; i < tamanho; i++)
        {
            soma += numeros[i] * peso;
            peso = peso == 2 ? 9 : peso - 1;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
