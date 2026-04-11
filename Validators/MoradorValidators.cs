using FluentValidation;
using SindiCore.API.DTOs.Requests;

namespace SindiCore.API.Validators;

public class CreateMoradorRequestValidator : AbstractValidator<CreateMoradorRequest>
{
    public CreateMoradorRequestValidator()
    {
        RuleFor(x => x.CondominioId)
            .NotEmpty().WithMessage("CondominioId é obrigatório");

        RuleFor(x => x.UnidadeId)
            .NotEmpty().WithMessage("UnidadeId é obrigatório");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");
    }
}

public class UpdateMoradorRequestValidator : AbstractValidator<UpdateMoradorRequest>
{
    public UpdateMoradorRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.UnidadeId)
            .NotEmpty().WithMessage("UnidadeId é obrigatório");
    }
}
