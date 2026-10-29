using System.Globalization;
using FluentValidation;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;

namespace SindiOps.API.Validators;

public class GerarRelatorioRequestValidator : AbstractValidator<GerarRelatorioRequest>
{
    public GerarRelatorioRequestValidator()
    {
        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("Tipo é obrigatório")
            .Must(t => RelatorioTipo.Todos.Contains(t))
            .WithMessage("Tipo de relatório inválido");

        RuleFor(x => x.CondominioId)
            .NotEmpty().WithMessage("CondominioId é obrigatório");

        RuleFor(x => x.Formato)
            .NotEmpty().WithMessage("Formato é obrigatório")
            .Must(f => RelatorioFormato.Todos.Contains(f))
            .WithMessage("Formato deve ser pdf, excel ou word");

        RuleFor(x => x.Filtros)
            .Must(PeriodoValido)
            .WithMessage("A data inicial deve ser igual ou anterior à data final");
    }

    private static bool PeriodoValido(Dictionary<string, string>? filtros)
    {
        if (filtros is null)
            return true;
        if (!TryDate(filtros, "dataInicio", out var inicio) || !TryDate(filtros, "dataFim", out var fim))
            return true;
        return fim >= inicio;
    }

    private static bool TryDate(Dictionary<string, string> filtros, string key, out DateOnly data)
    {
        data = default;
        return filtros.TryGetValue(key, out var valor) &&
               !string.IsNullOrWhiteSpace(valor) &&
               (DateOnly.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.None, out data) ||
                DateOnly.TryParse(valor, CultureInfo.GetCultureInfo("pt-BR"), DateTimeStyles.None, out data));
    }
}
