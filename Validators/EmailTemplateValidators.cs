using FluentValidation;
using SindiCore.API.Constants;
using SindiCore.API.DTOs.Requests;

namespace SindiCore.API.Validators;

public class CreateEmailTemplateRequestValidator : AbstractValidator<CreateEmailTemplateRequest>
{
    public CreateEmailTemplateRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório");

        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("Tipo é obrigatório")
            .Must(t => EmailTemplateTipo.Todos.Contains(t))
            .WithMessage("Tipo de template inválido");

        RuleFor(x => x.Assunto)
            .NotEmpty().WithMessage("Assunto é obrigatório");

        RuleFor(x => x.Corpo)
            .NotEmpty().WithMessage("Corpo é obrigatório");
    }
}

public class EmailTemplateQueryParamsValidator : AbstractValidator<EmailTemplateQueryParams>
{
    public EmailTemplateQueryParamsValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.Tipo), () =>
        {
            RuleFor(x => x.Tipo!)
                .Must(t => EmailTemplateTipo.Todos.Contains(t))
                .WithMessage("Tipo de template inválido");
        });
    }
}
