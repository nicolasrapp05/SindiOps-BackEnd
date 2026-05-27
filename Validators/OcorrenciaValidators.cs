using FluentValidation;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;

namespace SindiOps.API.Validators;

public class OcorrenciaQueryParamsValidator : AbstractValidator<OcorrenciaQueryParams>
{
    public OcorrenciaQueryParamsValidator()
    {
        RuleFor(x => x.CondominioId)
            .NotEmpty().WithMessage("condominioId é obrigatório");
    }
}

public class CreateOcorrenciaRequestValidator : AbstractValidator<CreateOcorrenciaRequest>
{
    public CreateOcorrenciaRequestValidator()
    {
        RuleFor(x => x.CondominioId)
            .NotEmpty().WithMessage("CondominioId é obrigatório");

        RuleFor(x => x.Origem)
            .NotEmpty().WithMessage("Origem é obrigatória")
            .Must(o => OcorrenciaOrigem.Todas.Contains(o))
            .WithMessage("Origem inválida");

        RuleFor(x => x.TipoLocal)
            .NotEmpty().WithMessage("TipoLocal é obrigatório")
            .Must(t => OcorrenciaTipoLocal.Todos.Contains(t))
            .WithMessage("Tipo de local inválido");

        RuleFor(x => x.TipoOcorrencia)
            .NotEmpty().WithMessage("TipoOcorrencia é obrigatório")
            .Must(t => OcorrenciaTipoOcorrencia.Todos.Contains(t))
            .WithMessage("Tipo de ocorrência inválido");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória");

        RuleFor(x => x.OcorreuEm)
            .NotEmpty().WithMessage("OcorreuEm é obrigatório");
    }
}

public class UpdateOcorrenciaStatusRequestValidator : AbstractValidator<UpdateOcorrenciaStatusRequest>
{
    private static readonly string[] StatusValidos =
    [
        OcorrenciaStatus.Nova, OcorrenciaStatus.EmAndamento,
        OcorrenciaStatus.Finalizada, OcorrenciaStatus.Cancelada
    ];

    public UpdateOcorrenciaStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status é obrigatório")
            .Must(s => StatusValidos.Contains(s))
            .WithMessage($"Status inválido. Valores aceitos: {string.Join(", ", StatusValidos)}");
    }
}

public class EnviarComunicacaoRequestValidator : AbstractValidator<EnviarComunicacaoRequest>
{
    public EnviarComunicacaoRequestValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("TemplateId é obrigatório");

        RuleFor(x => x.MoradorId)
            .NotEmpty().WithMessage("MoradorId é obrigatório");

        RuleFor(x => x.AssuntoEditado)
            .NotEmpty().WithMessage("AssuntoEditado é obrigatório");

        RuleFor(x => x.CorpoEditado)
            .NotEmpty().WithMessage("CorpoEditado é obrigatório");
    }
}
