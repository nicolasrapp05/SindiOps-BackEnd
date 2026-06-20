using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Infrastructure.Email;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class FuncionarioService : IFuncionarioService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly ISupabaseAuthService _supabaseAuth;
    private readonly IConviteResendRateLimiter _conviteResendRateLimiter;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FuncionarioService> _logger;

    public FuncionarioService(
        SindiOpsDbContext db,
        IMapper mapper,
        IEmailService emailService,
        ISupabaseAuthService supabaseAuth,
        IConviteResendRateLimiter conviteResendRateLimiter,
        IConfiguration configuration,
        ILogger<FuncionarioService> logger)
    {
        _db = db;
        _mapper = mapper;
        _emailService = emailService;
        _supabaseAuth = supabaseAuth;
        _conviteResendRateLimiter = conviteResendRateLimiter;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<FuncionarioResponse>> GetAllAsync(Guid sindicoId, string? cargo, bool? ativo)
    {
        var query = _db.Funcionarios
            .Include(f => f.CondominiosAcesso)
            .ThenInclude(fc => fc.Condominio)
            .Where(f => f.SindicoId == sindicoId);

        if (!string.IsNullOrWhiteSpace(cargo))
            query = query.Where(f => f.Cargo == cargo);

        if (ativo.HasValue)
            query = query.Where(f => f.Ativo == ativo.Value);

        var responses = _mapper.Map<List<FuncionarioResponse>>(
            await query.OrderBy(f => f.Nome).ToListAsync());

        await EnrichConviteStatusAsync(responses);
        return responses;
    }

    public async Task<FuncionarioResponse> GetByIdAsync(Guid id, Guid sindicoId)
    {
        var funcionario = await _db.Funcionarios
            .Include(f => f.CondominiosAcesso)
            .ThenInclude(fc => fc.Condominio)
            .FirstOrDefaultAsync(f => f.Id == id && f.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Funcionário não encontrado");

        var response = _mapper.Map<FuncionarioResponse>(funcionario);
        await EnrichConviteStatusAsync([response]);
        return response;
    }

    public async Task<FuncionarioResponse> ConvidarAsync(ConvidarFuncionarioRequest request, Guid sindicoId)
    {
        var email = request.Email.Trim();
        var emailNormalizado = email.ToLowerInvariant();
        var nome = request.Nome.Trim();

        var emailEmUso =
            await _db.Sindicos.AnyAsync(s => s.Email.ToLower() == emailNormalizado)
            || await _db.Funcionarios.AnyAsync(f => f.Email.ToLower() == emailNormalizado);

        if (emailEmUso)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Email), "Este email já está cadastrado no sistema.")
            });
        }

        await ValidarCondominiosDoSindicoAsync(request.CondominioIds, sindicoId);

        var funcionarioId = Guid.NewGuid();

        await _supabaseAuth.CreateUserWithPasswordAsync(
            funcionarioId,
            email,
            Guid.NewGuid().ToString("N") + "Aa1!",
            nome,
            request.Cargo);

        try
        {
            var funcionario = new Funcionario
            {
                Id = funcionarioId,
                SindicoId = sindicoId,
                Nome = nome,
                Email = email,
                Cargo = request.Cargo,
                SenhaHash = string.Empty,
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };

            _db.Funcionarios.Add(funcionario);
            await _db.SaveChangesAsync();

            await SincronizarCondominiosAcessoAsync(funcionario.Id, request.CondominioIds);

            var conviteEnviado = await EnviarConviteAsync(nome, email);

            var response = await CarregarResponseAsync(funcionario.Id, sindicoId);
            response.ConviteEnviado = conviteEnviado;
            response.ConvitePendente = true;
            return response;
        }
        catch
        {
            await _supabaseAuth.DeleteUserAsync(funcionarioId);
            throw;
        }
    }

    public async Task<FuncionarioResponse> ReenviarConviteAsync(Guid id, Guid sindicoId)
    {
        var funcionario = await _db.Funcionarios
            .FirstOrDefaultAsync(f => f.Id == id && f.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Funcionário não encontrado");

        if (!funcionario.Ativo)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(string.Empty, "Funcionário inativo não pode receber convite.")
            });
        }

        var lastSignIn = await _supabaseAuth.GetLastSignInAtAsync(funcionario.Id);
        if (lastSignIn is not null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(string.Empty, "Este funcionário já ativou o acesso.")
            });
        }

        if (!_conviteResendRateLimiter.TryAcquire(funcionario.Id))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(string.Empty, "Aguarde alguns minutos antes de reenviar o convite.")
            });
        }

        var conviteEnviado = await EnviarConviteAsync(funcionario.Nome, funcionario.Email);
        if (!conviteEnviado)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    string.Empty,
                    "Não foi possível enviar o email de convite. Tente novamente mais tarde.")
            });
        }

        var response = await CarregarResponseAsync(funcionario.Id, sindicoId);
        response.ConviteEnviado = true;
        response.ConvitePendente = true;
        return response;
    }

    public async Task<FuncionarioResponse> UpdateAsync(Guid id, UpdateFuncionarioRequest request, Guid sindicoId)
    {
        var funcionario = await _db.Funcionarios
            .FirstOrDefaultAsync(f => f.Id == id && f.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Funcionário não encontrado");

        await ValidarCondominiosDoSindicoAsync(request.CondominioIds, sindicoId);

        funcionario.Nome = request.Nome;
        funcionario.Cargo = request.Cargo;
        funcionario.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await SincronizarCondominiosAcessoAsync(funcionario.Id, request.CondominioIds);
        await _supabaseAuth.SyncUserMetadataAsync(funcionario.Id, funcionario.Nome, funcionario.Cargo);

        return await GetByIdAsync(funcionario.Id, sindicoId);
    }

    public async Task<FuncionarioResponse> AtivarAsync(Guid id, Guid sindicoId)
    {
        var funcionario = await _db.Funcionarios
            .FirstOrDefaultAsync(f => f.Id == id && f.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Funcionário não encontrado");

        funcionario.Ativo = true;
        funcionario.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetByIdAsync(funcionario.Id, sindicoId);
    }

    public async Task<FuncionarioResponse> DesativarAsync(Guid id, Guid sindicoId, Guid currentUserId)
    {
        if (id == currentUserId)
            throw new InvalidOperationException("O síndico não pode desativar a si mesmo");

        var funcionario = await _db.Funcionarios
            .FirstOrDefaultAsync(f => f.Id == id && f.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Funcionário não encontrado");

        funcionario.Ativo = false;
        funcionario.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetByIdAsync(funcionario.Id, sindicoId);
    }

    private async Task ValidarCondominiosDoSindicoAsync(IReadOnlyCollection<Guid> condominioIds, Guid sindicoId)
    {
        var ids = condominioIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("condominioIds", "Selecione ao menos um condomínio")
            });
        }

        var validos = await _db.Condominios
            .Where(c => c.SindicoId == sindicoId && ids.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        if (validos.Count != ids.Count)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("condominioIds", "Um ou mais condomínios são inválidos para este síndico")
            });
        }
    }

    private async Task SincronizarCondominiosAcessoAsync(Guid funcionarioId, IReadOnlyCollection<Guid> condominioIds)
    {
        var ids = condominioIds.Distinct().ToHashSet();

        var atuais = await _db.FuncionarioCondominios
            .Where(fc => fc.FuncionarioId == funcionarioId)
            .ToListAsync();

        var remover = atuais.Where(fc => !ids.Contains(fc.CondominioId)).ToList();
        if (remover.Count > 0)
            _db.FuncionarioCondominios.RemoveRange(remover);

        var existentes = atuais.Select(fc => fc.CondominioId).ToHashSet();
        foreach (var condominioId in ids.Where(id => !existentes.Contains(id)))
        {
            _db.FuncionarioCondominios.Add(new FuncionarioCondominio
            {
                FuncionarioId = funcionarioId,
                CondominioId = condominioId,
                CriadoEm = DateTime.UtcNow,
            });
        }

        await _db.SaveChangesAsync();
    }

    private async Task<FuncionarioResponse> CarregarResponseAsync(Guid funcionarioId, Guid sindicoId)
    {
        var funcionario = await _db.Funcionarios
            .Include(f => f.CondominiosAcesso)
            .ThenInclude(fc => fc.Condominio)
            .FirstAsync(f => f.Id == funcionarioId && f.SindicoId == sindicoId);

        return _mapper.Map<FuncionarioResponse>(funcionario);
    }

    private async Task EnrichConviteStatusAsync(IReadOnlyList<FuncionarioResponse> responses)
    {
        if (responses.Count == 0)
            return;

        var tasks = responses.Select(async response =>
        {
            var lastSignIn = await _supabaseAuth.GetLastSignInAtAsync(response.Id);
            response.ConvitePendente = lastSignIn is null;
        });

        await Task.WhenAll(tasks);
    }

    private async Task<bool> EnviarConviteAsync(string nome, string email)
    {
        var frontendUrl = GetFrontendUrl().TrimEnd('/');
        var redirectTo = $"{frontendUrl}/primeiro-acesso";

        var recovery = await _supabaseAuth.GenerateRecoveryLinkAsync(email, redirectTo);
        if (recovery is null)
        {
            _logger.LogWarning("Falha ao gerar link de primeiro acesso para {Email}", email);
            return false;
        }

        var (htmlContent, plainBody) = FuncionarioInviteEmailBuilder.Build(
            nome, email, recovery.EmailOtp, frontendUrl, recovery.HashedToken);

        return await _emailService.SendAuthHtmlAsync(
            email,
            "Seu acesso ao SindiOps está pronto",
            htmlContent,
            plainBody);
    }

    private string GetFrontendUrl() => FrontendUrlResolver.Resolve(_configuration);
}
