using FluentValidation;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.Helpers;

namespace SindiOps.API.Validators;

public class CreateFornecedorRequestValidator : AbstractValidator<CreateFornecedorRequest>
{
    public CreateFornecedorRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Cnpj)
            .Must(CnpjValidador.EhValido)
            .WithMessage("CNPJ inválido");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email inválido")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleForEach(x => x.Servicos).SetValidator(new CreateServicoRequestValidator());
    }
}

public class CreateServicoRequestValidator : AbstractValidator<CreateServicoRequest>
{
    public CreateServicoRequestValidator()
    {
        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("Tipo do serviço é obrigatório");
    }
}
