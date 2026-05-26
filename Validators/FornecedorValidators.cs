using System.Text.RegularExpressions;
using FluentValidation;
using SindiCore.API.DTOs.Requests;

namespace SindiCore.API.Validators;

public class CreateFornecedorRequestValidator : AbstractValidator<CreateFornecedorRequest>
{
    public CreateFornecedorRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Cnpj)
            .Must(cnpj => string.IsNullOrEmpty(cnpj) || CnpjValido(cnpj))
            .WithMessage("CNPJ inválido — informe 14 dígitos numéricos");

        RuleForEach(x => x.Servicos).SetValidator(new CreateServicoRequestValidator());
    }

    private static bool CnpjValido(string cnpj)
    {
        var soDigitos = Regex.Replace(cnpj, @"\D", "");
        return soDigitos.Length == 14;
    }
}

public class CreateServicoRequestValidator : AbstractValidator<CreateServicoRequest>
{
    public CreateServicoRequestValidator()
    {
        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("Tipo do serviço é obrigatório");
    }
}
