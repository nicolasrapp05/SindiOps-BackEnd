using FluentValidation;
using SindiCore.API.Constants;
using SindiCore.API.DTOs.Requests;

namespace SindiCore.API.Validators;

public class ConvidarFuncionarioRequestValidator : AbstractValidator<ConvidarFuncionarioRequest>
{
    private static readonly string[] CargosValidos =
        [CargoConstants.Zelador, CargoConstants.Secretario, CargoConstants.Porteiro, CargoConstants.Outro];

    public ConvidarFuncionarioRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Cargo)
            .NotEmpty().WithMessage("Cargo é obrigatório")
            .Must(c => CargosValidos.Contains(c))
            .WithMessage($"Cargo inválido. Valores aceitos: {string.Join(", ", CargosValidos)}");
    }
}

public class UpdateFuncionarioRequestValidator : AbstractValidator<UpdateFuncionarioRequest>
{
    private static readonly string[] CargosValidos =
        [CargoConstants.Zelador, CargoConstants.Secretario, CargoConstants.Porteiro, CargoConstants.Outro];

    public UpdateFuncionarioRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Cargo)
            .NotEmpty().WithMessage("Cargo é obrigatório")
            .Must(c => CargosValidos.Contains(c))
            .WithMessage($"Cargo inválido. Valores aceitos: {string.Join(", ", CargosValidos)}");
    }
}
