using System.Net;
using System.Text.Json;
using FluentValidation;
using SindiOps.API.Helpers;

namespace SindiOps.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, response) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.UnprocessableEntity,
                ApiResponse<object>.Fail(
                    "Erro de validação",
                    ve.Errors.Select(e => new ApiError
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage
                    }).ToList()
                )
            ),
            KeyNotFoundException knfe => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.Fail(knfe.Message)
            ),
            UnauthorizedAccessException uae => (
                HttpStatusCode.Forbidden,
                ApiResponse<object>.Fail(uae.Message)
            ),
            InvalidOperationException ioe => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(ioe.Message)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail("Erro interno do servidor")
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
