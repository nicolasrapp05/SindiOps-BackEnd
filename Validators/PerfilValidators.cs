using FluentValidation;
using SindiOps.API.DTOs.Requests;

namespace SindiOps.API.Validators;

public class UpdatePerfilRequestValidator : AbstractValidator<UpdatePerfilRequest>
{
    public UpdatePerfilRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(2).WithMessage("Nome deve ter pelo menos 2 caracteres")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");
    }
}
