using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Infrastructure.Email;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class FuncionarioService : IFuncionarioService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FuncionarioService> _logger;

    public FuncionarioService(
        SindiOpsDbContext db,
        IMapper mapper,
        IEmailService emailService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<FuncionarioService> logger)
    {
        _db = db;
        _mapper = mapper;
        _emailService = emailService;
        _httpClientFactory = httpClientFactory;
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

        return _mapper.Map<List<FuncionarioResponse>>(
            await query.OrderBy(f => f.Nome).ToListAsync());
    }

    public async Task<FuncionarioResponse> GetByIdAsync(Guid id, Guid sindicoId)
    {
        var funcionario = await _db.Funcionarios
            .Include(f => f.CondominiosAcesso)
            .ThenInclude(fc => fc.Condominio)
            .FirstOrDefaultAsync(f => f.Id == id && f.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Funcionário não encontrado");

        return _mapper.Map<FuncionarioResponse>(funcionario);
    }

    public async Task<FuncionarioResponse> ConvidarAsync(ConvidarFuncionarioRequest request, Guid sindicoId)
    {
        var emailExiste = await _db.Funcionarios.AnyAsync(f => f.Email == request.Email);
        if (emailExiste)
            throw new ValidationException(new[]
            {
                new ValidationFailure("email", "Email já cadastrado no sistema")
            });

        await ValidarCondominiosDoSindicoAsync(request.CondominioIds, sindicoId);

        var funcionario = new Funcionario
        {
            SindicoId = sindicoId,
            Nome = request.Nome,
            Email = request.Email,
            Cargo = request.Cargo,
            SenhaHash = string.Empty,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _db.Funcionarios.Add(funcionario);
        await _db.SaveChangesAsync();

        await SincronizarCondominiosAcessoAsync(funcionario.Id, request.CondominioIds);

        await CriarUsuarioSupabaseAsync(request.Email, request.Nome);

        var conviteEnviado = await EnviarConviteAsync(request.Nome, request.Email);

        var response = await CarregarResponseAsync(funcionario.Id, sindicoId);
        response.ConviteEnviado = conviteEnviado;
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

        return await CarregarResponseAsync(funcionario.Id, sindicoId);
    }

    public async Task AtivarAsync(Guid id, Guid sindicoId)
    {
        var funcionario = await _db.Funcionarios
            .FirstOrDefaultAsync(f => f.Id == id && f.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Funcionário não encontrado");

        funcionario.Ativo = true;
        funcionario.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task DesativarAsync(Guid id, Guid sindicoId, Guid currentUserId)
    {
        if (id == currentUserId)
            throw new InvalidOperationException("O síndico não pode desativar a si mesmo");

        var funcionario = await _db.Funcionarios
            .FirstOrDefaultAsync(f => f.Id == id && f.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Funcionário não encontrado");

        funcionario.Ativo = false;
        funcionario.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private async Task ValidarCondominiosDoSindicoAsync(IReadOnlyCollection<Guid> condominioIds, Guid sindicoId)
    {
        var ids = condominioIds.Distinct().ToList();
        if (ids.Count == 0)
            throw new ValidationException(new[]
            {
                new ValidationFailure("condominioIds", "Selecione ao menos um condomínio")
            });

        var validos = await _db.Condominios
            .Where(c => c.SindicoId == sindicoId && ids.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        if (validos.Count != ids.Count)
            throw new ValidationException(new[]
            {
                new ValidationFailure("condominioIds", "Um ou mais condomínios são inválidos para este síndico")
            });
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

    private async Task CriarUsuarioSupabaseAsync(string email, string nome)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["Supabase:Url"]}/auth/v1/admin/users";
            var serviceKey = _configuration["Supabase:ServiceRoleKey"]!;

            var body = JsonSerializer.Serialize(new
            {
                email,
                password = Guid.NewGuid().ToString("N") + "Aa1!",
                email_confirm = true,
                user_metadata = new { full_name = nome }
            });

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceKey);
            req.Headers.Add("apikey", serviceKey);
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(req);

            if (!response.IsSuccessStatusCode && (int)response.StatusCode != 422)
            {
                var err = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Supabase Auth: falha ao criar usuário {Email} | status {Status} | {Error}",
                    email, response.StatusCode, err);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao chamar Supabase Auth Admin API para {Email}", email);
        }
    }

    private async Task<bool> EnviarConviteAsync(string nome, string email)
    {
        var frontendUrl = _configuration["Cors:AllowedOrigin"] ?? "https://app.sindiops.com.br";
        var htmlBody = $"""
            <h2>Bem-vindo ao SíndiOps!</h2>
            <p>Olá, <strong>{nome}</strong>.</p>
            <p>Você foi cadastrado como funcionário na plataforma SíndiOps.</p>
            <p>Clique no link abaixo para definir sua senha e acessar o sistema:</p>
            <p><a href="{frontendUrl}/primeiro-acesso">Definir minha senha</a></p>
            <p>Se você não reconhece este email, ignore esta mensagem.</p>
            """;

        return await _emailService.SendAsync(email, "Seu acesso ao SíndiOps está pronto", htmlBody);
    }
}
