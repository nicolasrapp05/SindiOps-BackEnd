using FluentValidation;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;

namespace SindiOps.API.Validators;

public class SolicitacaoCompraQueryParamsValidator : AbstractValidator<SolicitacaoCompraQueryParams>
{
    public SolicitacaoCompraQueryParamsValidator()
    {
        RuleFor(x => x.CondominioId)
            .NotEmpty().WithMessage("condominioId é obrigatório");
    }
}

public class CreateSolicitacaoCompraRequestValidator : AbstractValidator<CreateSolicitacaoCompraRequest>
{
    public CreateSolicitacaoCompraRequestValidator()
    {
        RuleFor(x => x.CondominioId)
            .NotEmpty().WithMessage("CondominioId é obrigatório");

        RuleFor(x => x.Categoria)
            .NotEmpty().WithMessage("Categoria é obrigatória")
            .Must(c => SolicitacaoCompraCategoria.Todas.Contains(c))
            .WithMessage("Categoria inválida");

        RuleFor(x => x.Item)
            .NotEmpty().WithMessage("Item é obrigatório");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.TipoAprovacao)
            .NotEmpty().WithMessage("TipoAprovacao é obrigatório")
            .Must(t => TipoAprovacaoCompra.Todos.Contains(t))
            .WithMessage("Tipo de aprovação inválido");
    }
}

public class UpdateSolicitacaoCompraStatusRequestValidator : AbstractValidator<UpdateSolicitacaoCompraStatusRequest>
{
    private static readonly string[] StatusValidos =
    [
        SolicitacaoStatus.Nova, SolicitacaoStatus.EmAndamento,
        SolicitacaoStatus.Finalizada, SolicitacaoStatus.Cancelada
    ];

    public UpdateSolicitacaoCompraStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status é obrigatório")
            .Must(s => StatusValidos.Contains(s))
            .WithMessage($"Status inválido. Valores aceitos: {string.Join(", ", StatusValidos)}");
    }
}

public class CreateCotacaoRequestValidator : AbstractValidator<CreateCotacaoRequest>
{
    public CreateCotacaoRequestValidator()
    {
        RuleFor(x => x.NomeEmpresa)
            .NotEmpty()
            .When(x => !x.FornecedorId.HasValue)
            .WithMessage("NomeEmpresa é obrigatório quando FornecedorId não é informado");

        RuleFor(x => x.NomeEmpresa)
            .Empty()
            .When(x => x.FornecedorId.HasValue)
            .WithMessage("NomeEmpresa não deve ser informado quando FornecedorId é informado");

        RuleFor(x => x.ValorUnitario)
            .GreaterThan(0).WithMessage("ValorUnitario deve ser maior que zero");
    }
}
