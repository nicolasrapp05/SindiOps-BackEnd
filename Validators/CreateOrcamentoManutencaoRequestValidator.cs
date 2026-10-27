using FluentValidation;
using SindiOps.API.DTOs.Requests;

namespace SindiOps.API.Validators;

public class CreateOrcamentoManutencaoRequestValidator : AbstractValidator<CreateOrcamentoManutencaoRequest>
{
    public CreateOrcamentoManutencaoRequestValidator()
    {
        RuleFor(x => x.NomeEmpresa)
            .NotEmpty()
            .When(x => !x.FornecedorId.HasValue || x.FornecedorId == Guid.Empty)
            .WithMessage("NomeEmpresa é obrigatório quando FornecedorId não é informado");

        RuleFor(x => x.NomeEmpresa)
            .Must(nome => string.IsNullOrWhiteSpace(nome))
            .When(x => x.FornecedorId.HasValue && x.FornecedorId != Guid.Empty)
            .WithMessage("NomeEmpresa não deve ser informado quando FornecedorId é informado");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

        RuleFor(x => x.PrazoExecucaoDias)
            .GreaterThan(0)
            .When(x => x.PrazoExecucaoDias.HasValue)
            .WithMessage("PrazoExecucaoDias deve ser maior que zero");
    }
}
