using FluentValidation;
using SindiCore.API.Constants;
using SindiCore.API.DTOs.Requests;

namespace SindiCore.API.Validators;

public class ManutencaoObrigatoriaQueryParamsValidator : AbstractValidator<ManutencaoObrigatoriaQueryParams>
{
    public ManutencaoObrigatoriaQueryParamsValidator()
    {
        RuleFor(x => x.CondominioId)
            .NotEmpty().WithMessage("condominioId é obrigatório");
    }
}

public class CreateManutencaoObrigatoriaRequestValidator : AbstractValidator<CreateManutencaoObrigatoriaRequest>
{
    public CreateManutencaoObrigatoriaRequestValidator()
    {
        RuleFor(x => x.CondominioId)
            .NotEmpty().WithMessage("CondominioId é obrigatório");

        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("Tipo é obrigatório")
            .Must(t => ManutencaoObrigatoriaTipo.Todos.Contains(t))
            .WithMessage("Tipo de manutenção inválido");

        RuleFor(x => x.DataVencimento)
            .NotEmpty().WithMessage("DataVencimento é obrigatória");
    }
}

public class RealizarManutencaoRequestValidator : AbstractValidator<RealizarManutencaoRequest>
{
    public RealizarManutencaoRequestValidator()
    {
        RuleFor(x => x.DataRealizacao)
            .Must(d => d != default)
            .WithMessage("DataRealizacao é obrigatória")
            .Must(d => d <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Data de realização no futuro não é permitida")
            .OverridePropertyName("dataRealizacao");
    }
}
