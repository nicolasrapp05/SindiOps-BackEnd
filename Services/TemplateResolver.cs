using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class TemplateResolver : ITemplateResolver
{
    public string Resolve(string template, Dictionary<string, string> values, string? propertyNameForErrors = null)
    {
        var result = template;
        var field = propertyNameForErrors ?? "template";

        foreach (var (key, value) in values)
            result = result.Replace($"{{{{{key}}}}}", value);

        var unresolvedTokens = Regex.Matches(result, @"\{\{[^}]+\}\}")
            .Select(m => m.Value)
            .Distinct()
            .ToList();

        if (unresolvedTokens.Count > 0)
            throw new ValidationException(
                unresolvedTokens.Select(token => new ValidationFailure(field,
                    $"Variável não resolvida no template: {token}")).ToList());

        return result;
    }
}
