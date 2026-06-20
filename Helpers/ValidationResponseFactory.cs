using Microsoft.AspNetCore.Mvc;

namespace SindiOps.API.Helpers;

public static class ValidationResponseFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error => new ApiError
            {
                Field = NormalizeFieldName(entry.Key),
                Message = string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Valor inválido"
                    : error.ErrorMessage,
            }))
            .ToList();

        var response = ApiResponse<object>.Fail("Erro de validação", errors);
        return new UnprocessableEntityObjectResult(response);
    }

    private static string NormalizeFieldName(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        var field = key;
        var lastDot = field.LastIndexOf('.');
        if (lastDot >= 0)
            field = field[(lastDot + 1)..];

        if (field.StartsWith('$'))
            field = field.TrimStart('$', '.');

        return field;
    }
}
