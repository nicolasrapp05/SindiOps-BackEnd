using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
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
    private readonly ISupabaseAuthService _supabaseAuth;
    private readonly IConfiguration _configuration;

    public FuncionarioService(
        SindiOpsDbContext db,
        IMapper mapper,
        IEmailService emailService,
        ISupabaseAuthService supabaseAuth,
        IConfiguration configuration)
    {
        _db = db;
        _mapper = mapper;
        _emailService = emailService;
        _supabaseAuth = supabaseAuth;
        _configuration = configuration;
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

        await _supabaseAuth.CreateUserAsync(funcionario.Id, request.Email, request.Nome, request.Cargo);

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
        await _supabaseAuth.SyncUserMetadataAsync(funcionario.Id, funcionario.Nome, funcionario.Cargo);

        return await CarregarResponseAsync(funcionario.Id, sindicoId);
    }

    public async Task<FuncionarioResponse> AtivarAsync(Guid id, Guid sindicoId)
    {
        var funcionario = await _db.Funcionarios
            .FirstOrDefaultAsync(f => f.Id == id && f.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Funcionário não encontrado");

        funcionario.Ativo = true;
        funcionario.AtualizadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await CarregarResponseAsync(funcionario.Id, sindicoId);
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

        return await CarregarResponseAsync(funcionario.Id, sindicoId);
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
