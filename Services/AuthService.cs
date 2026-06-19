using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Infrastructure.Auth;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Infrastructure.Email;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class AuthService : IAuthService
{
    private readonly SindiOpsDbContext _db;
    private readonly ISupabaseAuthService _supabaseAuth;
    private readonly IEmailService _emailService;
    private readonly IPasswordResetRateLimiter _passwordResetRateLimiter;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        SindiOpsDbContext db,
        ISupabaseAuthService supabaseAuth,
        IEmailService emailService,
        IPasswordResetRateLimiter passwordResetRateLimiter,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _db = db;
        _supabaseAuth = supabaseAuth;
        _emailService = emailService;
        _passwordResetRateLimiter = passwordResetRateLimiter;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CadastroSindicoResponse> CadastroSindicoAsync(CadastroSindicoRequest request)
    {
        var email = request.Email.Trim();
        var emailNormalizado = email.ToLowerInvariant();

        var emailEmUso =
            await _db.Sindicos.AnyAsync(s => s.Email.ToLower() == emailNormalizado)
            || await _db.Funcionarios.AnyAsync(f => f.Email.ToLower() == emailNormalizado);

        if (emailEmUso)
        {
            throw new FluentValidation.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Email), "Este email já está cadastrado no sistema.")
            });
        }

        var id = Guid.NewGuid();
        var nome = request.Nome.Trim();

        await _supabaseAuth.CreateUserWithPasswordAsync(
            id, email, request.Senha, nome, Constants.CargoConstants.Sindico);

        var sindico = new Entities.Sindico
        {
            Id = id,
            Nome = nome,
            Email = email,
            CriadoEm = DateTime.UtcNow,
        };

        _db.Sindicos.Add(sindico);
        await _db.SaveChangesAsync();

        return new CadastroSindicoResponse
        {
            Id = id,
            Nome = nome,
            Email = email,
        };
    }

    public async Task EsqueciSenhaAsync(EsqueciSenhaRequest request)
    {
        var email = request.Email.Trim();
        var clientIp = ClientIpResolver.Resolve(_httpContextAccessor.HttpContext);

        if (!_passwordResetRateLimiter.TryAcquire(email, clientIp))
        {
            _logger.LogWarning(
                "Recuperação de senha bloqueada por rate limit | ip {ClientIp}",
                clientIp);
            return;
        }

        var frontendUrl = GetFrontendUrl().TrimEnd('/');
        var redirectTo = $"{frontendUrl}/redefinir-senha";

        var recovery = await _supabaseAuth.GenerateRecoveryLinkAsync(email, redirectTo);
        if (recovery is null)
            return;

        var (htmlContent, plainBody) = PasswordResetEmailBuilder.Build(email, recovery.EmailOtp, frontendUrl);
        var sent = await _emailService.SendAuthHtmlAsync(
            email,
            "Redefinir sua senha — SindiOps",
            htmlContent,
            plainBody);

        if (!sent)
        {
            _logger.LogWarning("Falha ao enviar email de recuperação de senha para {Email}", email);
        }
    }

    private string GetFrontendUrl() =>
        _configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173";
}
