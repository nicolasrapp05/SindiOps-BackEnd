using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Infrastructure.Auth;

public class SupabaseAuthService : ISupabaseAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SupabaseAuthService> _logger;

    public SupabaseAuthService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SupabaseAuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public Task CreateUserAsync(Guid userId, string email, string nome, string cargo) =>
        SendAsync(
            HttpMethod.Post,
            $"{BaseUrl}/auth/v1/admin/users",
            new
            {
                id = userId,
                email,
                password = Guid.NewGuid().ToString("N") + "Aa1!",
                email_confirm = true,
                user_metadata = BuildMetadata(nome, cargo),
            },
            allowConflict: true,
            context: $"criar usuário {email}");

    public Task CreateUserWithPasswordAsync(
        Guid userId, string email, string password, string nome, string cargo) =>
        SendRequiredAsync(
            HttpMethod.Post,
            $"{BaseUrl}/auth/v1/admin/users",
            new
            {
                id = userId,
                email,
                password,
                email_confirm = true,
                user_metadata = BuildMetadata(nome, cargo),
            },
            duplicateEmailField: nameof(email),
            context: $"cadastrar síndico {email}");

    public Task SyncUserMetadataAsync(Guid userId, string nome, string cargo) =>
        SendAsync(
            HttpMethod.Put,
            $"{BaseUrl}/auth/v1/admin/users/{userId}",
            new { user_metadata = BuildMetadata(nome, cargo) },
            allowConflict: false,
            context: $"sincronizar metadata do usuário {userId}");

    public async Task<RecoveryLinkData?> GenerateRecoveryLinkAsync(string email, string redirectTo)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var payload = JsonSerializer.Serialize(new
            {
                type = "recovery",
                email = email.Trim(),
                redirect_to = redirectTo,
            });

            using var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/auth/v1/admin/generate_link");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ServiceKey);
            req.Headers.Add("apikey", ServiceKey);
            req.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(req);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Supabase Auth: falha ao gerar link de recuperação | status {Status}",
                    response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var props = doc.RootElement.TryGetProperty("properties", out var nested)
                ? nested
                : doc.RootElement;

            if (!props.TryGetProperty("email_otp", out var otpProp))
                return null;

            var otp = otpProp.GetString();
            if (string.IsNullOrWhiteSpace(otp))
                return null;

            props.TryGetProperty("hashed_token", out var hashProp);
            var hashedToken = hashProp.GetString() ?? string.Empty;

            return new RecoveryLinkData(otp, hashedToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Supabase Auth: erro ao gerar link de recuperação para {Email}", email);
            return null;
        }
    }

    private string BaseUrl => _configuration["Supabase:Url"]!.TrimEnd('/');

    private string ServiceKey => _configuration["Supabase:ServiceRoleKey"]!;

    private static object BuildMetadata(string nome, string cargo) => new
    {
        nome,
        full_name = nome,
        cargo,
    };

    private Task SendRequiredAsync(
        HttpMethod method,
        string url,
        object body,
        string duplicateEmailField,
        string context) =>
        SendAsync(method, url, body, allowConflict: false, context, throwOnFailure: true, duplicateEmailField);

    private async Task SendAsync(
        HttpMethod method,
        string url,
        object body,
        bool allowConflict,
        string context,
        bool throwOnFailure = false,
        string? duplicateEmailField = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var payload = JsonSerializer.Serialize(body);

            using var req = new HttpRequestMessage(method, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ServiceKey);
            req.Headers.Add("apikey", ServiceKey);
            req.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(req);

            if (response.IsSuccessStatusCode)
                return;

            if (allowConflict && (int)response.StatusCode is 409 or 422)
                return;

            var err = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "Supabase Auth: falha ao {Context} | status {Status} | {Error}",
                context, response.StatusCode, err);

            if (throwOnFailure)
            {
                if (duplicateEmailField is not null && (int)response.StatusCode is 409 or 422)
                {
                    throw new ValidationException(new[]
                    {
                        new ValidationFailure(
                            duplicateEmailField,
                            "Este email já está cadastrado no sistema.")
                    });
                }

                throw new ValidationException(new[]
                {
                    new ValidationFailure(
                        string.Empty,
                        "Não foi possível criar a conta. Tente novamente mais tarde.")
                });
            }
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Supabase Auth: erro ao {Context}", context);
            if (throwOnFailure)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(
                        string.Empty,
                        "Não foi possível criar a conta. Tente novamente mais tarde.")
                });
            }
        }
    }
}
