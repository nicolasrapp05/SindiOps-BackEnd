using FluentValidation;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;

namespace SindiOps.API.Validators;

public class CreateContratoRequestValidator : AbstractValidator<CreateContratoRequest>
{
    public CreateContratoRequestValidator()
    {
        RuleFor(x => x.CondominioId)
            .NotEmpty().WithMessage("CondominioId é obrigatório");

        RuleFor(x => x.FornecedorId)
            .NotEmpty().WithMessage("FornecedorId é obrigatório");

        RuleFor(x => x.TipoServico)
            .NotEmpty().WithMessage("TipoServico é obrigatório")
            .MaximumLength(200);

        RuleFor(x => x)
            .Must(x => x.DataInicio == null || x.DataFim == null || x.DataFim >= x.DataInicio)
            .WithMessage("DataFim deve ser igual ou posterior a DataInicio");
    }
}

public class UpdateContratoStatusRequestValidator : AbstractValidator<UpdateContratoStatusRequest>
{
    public UpdateContratoStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status é obrigatório")
            .Must(s => s == ContratoStatus.Cancelled)
            .WithMessage("Use este endpoint apenas para cancelar o contrato.");
    }
}
