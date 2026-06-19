using FluentValidation;
using SindiOps.API.DTOs.Requests;

namespace SindiOps.API.Validators;

public class EsqueciSenhaRequestValidator : AbstractValidator<EsqueciSenhaRequest>
{
    public EsqueciSenhaRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Formato de email inválido")
            .MaximumLength(200).WithMessage("Email deve ter no máximo 200 caracteres");
    }
}
