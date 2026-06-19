using FluentValidation;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;

namespace SindiOps.API.Validators;

public class EmailLogQueryParamsValidator : AbstractValidator<EmailLogQueryParams>
{
    public EmailLogQueryParamsValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.StatusEntrega), () =>
        {
            RuleFor(x => x.StatusEntrega!)
                .Must(s => EmailLogStatusTodos.Contains(s))
                .WithMessage("statusEntrega inválido");
        });

        When(x => !string.IsNullOrWhiteSpace(x.TemplateTipo), () =>
        {
            RuleFor(x => x.TemplateTipo!)
                .Must(t => EmailTemplateTipo.Todos.Contains(t))
                .WithMessage("templateTipo inválido");
        });
    }

    private static readonly HashSet<string> EmailLogStatusTodos =
    [
        EmailLogStatus.Sent,
        EmailLogStatus.Delivered,
        EmailLogStatus.Failed
    ];
}
