using FluentValidation;
using SindiCore.API.Constants;
using SindiCore.API.DTOs.Requests;

namespace SindiCore.API.Validators;

public class SolicitacaoManutencaoQueryParamsValidator : AbstractValidator<SolicitacaoManutencaoQueryParams>
{
    public SolicitacaoManutencaoQueryParamsValidator()
    {
        RuleFor(x => x.CondominioId)
            .NotEmpty().WithMessage("condominioId é obrigatório");
    }
}

public class CreateSolicitacaoManutencaoRequestValidator : AbstractValidator<CreateSolicitacaoManutencaoRequest>
{
    public CreateSolicitacaoManutencaoRequestValidator()
    {
        RuleFor(x => x.CondominioId)
            .NotEmpty().WithMessage("CondominioId é obrigatório");

        RuleFor(x => x.TipoServico)
            .NotEmpty().WithMessage("TipoServico é obrigatório")
            .Must(t => SolicitacaoManutencaoTipo.Todos.Contains(t))
            .WithMessage("Tipo de serviço inválido");

        RuleFor(x => x.Local)
            .NotEmpty().WithMessage("Local é obrigatório");

        RuleFor(x => x.Responsavel)
            .NotEmpty().WithMessage("Responsavel é obrigatório")
            .Must(r => ResponsavelSolicitacao.Todos.Contains(r))
            .WithMessage("Responsavel deve ser 'fornecedor' ou 'zelador'");

        RuleFor(x => x.FornecedorId)
            .NotEmpty()
            .When(x => x.Responsavel == ResponsavelSolicitacao.Fornecedor)
            .WithMessage("FornecedorId é obrigatório quando Responsavel é 'fornecedor'");
    }
}

public class UpdateSolicitacaoStatusRequestValidator : AbstractValidator<UpdateSolicitacaoStatusRequest>
{
    private static readonly string[] StatusValidos =
    [
        SolicitacaoStatus.Nova, SolicitacaoStatus.EmAndamento,
        SolicitacaoStatus.Finalizada, SolicitacaoStatus.Cancelada
    ];

    public UpdateSolicitacaoStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status é obrigatório")
            .Must(s => StatusValidos.Contains(s))
            .WithMessage($"Status inválido. Valores aceitos: {string.Join(", ", StatusValidos)}");

        RuleFor(x => x.DataConclusao)
            .Must((req, d) => req.Status != SolicitacaoStatus.Finalizada || (d.HasValue && d.Value != default))
            .WithMessage("DataConclusao é obrigatória quando Status é 'finalizada'");
    }
}
