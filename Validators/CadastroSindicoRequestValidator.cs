using FluentValidation;
using SindiOps.API.DTOs.Requests;

namespace SindiOps.API.Validators;

public class CadastroSindicoRequestValidator : AbstractValidator<CadastroSindicoRequest>
{
    public CadastroSindicoRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Formato de email inválido")
            .MaximumLength(200).WithMessage("Email deve ter no máximo 200 caracteres");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres");

        RuleFor(x => x.ConfirmarSenha)
            .Equal(x => x.Senha).WithMessage("As senhas não coincidem");
    }
}
