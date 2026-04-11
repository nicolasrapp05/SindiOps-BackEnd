using FluentValidation;
using SindiCore.API.Constants;
using SindiCore.API.DTOs.Requests;

namespace SindiCore.API.Validators;

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
    }

    private static readonly HashSet<string> EmailLogStatusTodos =
    [
        EmailLogStatus.Sent,
        EmailLogStatus.Delivered,
        EmailLogStatus.Failed
    ];
}
