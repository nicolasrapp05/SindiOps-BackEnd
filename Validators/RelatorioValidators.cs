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
    }
}
