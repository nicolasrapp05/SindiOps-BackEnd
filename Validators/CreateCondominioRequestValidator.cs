using FluentValidation;
using SindiCore.API.DTOs.Requests;

namespace SindiCore.API.Validators;

public class CreateCondominioRequestValidator : AbstractValidator<CreateCondominioRequest>
{
    public CreateCondominioRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.EnderecoCep)
            .Matches(@"^\d{5}-?\d{3}$").WithMessage("CEP inválido")
            .When(x => !string.IsNullOrEmpty(x.EnderecoCep));

        RuleFor(x => x.VencimentoMandato)
            .GreaterThan(x => x.DataEleicao)
            .WithMessage("Vencimento do mandato deve ser posterior à data de eleição")
            .When(x => x.DataEleicao.HasValue && x.VencimentoMandato.HasValue);
    }
}

public class CreateBlocoRequestValidator : AbstractValidator<CreateBlocoRequest>
{
    public CreateBlocoRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do bloco é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");
    }
}

public class CreateUnidadeRequestValidator : AbstractValidator<CreateUnidadeRequest>
{
    public CreateUnidadeRequestValidator()
    {
        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage("Número da unidade é obrigatório")
            .MaximumLength(20).WithMessage("Número deve ter no máximo 20 caracteres");
    }
}

public class UpdateBlocoRequestValidator : AbstractValidator<UpdateBlocoRequest>
{
    public UpdateBlocoRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do bloco é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");
    }
}

public class UpdateUnidadeRequestValidator : AbstractValidator<UpdateUnidadeRequest>
{
    public UpdateUnidadeRequestValidator()
    {
        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage("Número da unidade é obrigatório")
            .MaximumLength(20).WithMessage("Número deve ter no máximo 20 caracteres");
    }
}
