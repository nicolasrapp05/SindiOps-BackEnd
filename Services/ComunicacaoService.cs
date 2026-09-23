using System.Globalization;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Infrastructure.Email;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class ComunicacaoService : IComunicacaoService
{
    private readonly SindiOpsDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ITemplateResolver _templateResolver;

    public ComunicacaoService(
        SindiOpsDbContext db,
        IEmailService emailService,
        ITemplateResolver templateResolver)
    {
        _db = db;
        _emailService = emailService;
        _templateResolver = templateResolver;
    }

    public async Task<ComunicacaoResponse> EnviarComunicacaoAsync(
        Guid ocorrenciaId,
        EnviarComunicacaoRequest request,
        Guid enviadoPorId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, enviadoPorId);

        if (!await UsuarioSindicoScope.IsFuncionarioOuSindicoPrincipalAsync(_db, enviadoPorId, sindicoId))
            throw new ValidationException(new[]
            {
                new ValidationFailure("", "Utilizador não autorizado a enviar comunicações")
            });

        var ocorrencia = await _db.Ocorrencias
            .Include(o => o.Condominio)
            .Include(o => o.Morador)!.ThenInclude(m => m!.Pessoa)
            .Include(o => o.Morador)!.ThenInclude(m => m!.Unidade).ThenInclude(u => u!.Bloco)
            .FirstOrDefaultAsync(o => o.Id == ocorrenciaId && o.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Ocorrência não encontrada");

        var template = await _db.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Template não encontrado");

        var morador = await _db.Moradores
            .Include(m => m.Pessoa)
            .Include(m => m.Unidade)
            .ThenInclude(u => u.Bloco)
            .FirstOrDefaultAsync(m => m.Id == request.MoradorId && m.Unidade.CondominioId == ocorrencia.CondominioId)
            ?? throw new KeyNotFoundException("Morador não encontrado neste condomínio");

        var sindico = await _db.Usuarios
            .Include(u => u.Pessoa)
            .FirstOrDefaultAsync(s => s.Id == sindicoId && s.Cargo == CargoConstants.Sindico)
            ?? throw new KeyNotFoundException("Síndico não encontrado");

        var valores = MontarVariaveisTemplate(ocorrencia, morador, sindico, request.ValorMulta, request.PrazoResposta);

        var assunto = _templateResolver.Resolve(request.AssuntoEditado, valores, nameof(EnviarComunicacaoRequest.AssuntoEditado));
        var corpo = _templateResolver.Resolve(request.CorpoEditado, valores, nameof(EnviarComunicacaoRequest.CorpoEditado));

        var enviadoOk = false;
        try
        {
            enviadoOk = await _emailService.SendAsync(morador.Pessoa.Email, assunto, corpo);
        }
        catch
        {
            enviadoOk = false;
        }

        var log = new EmailLog
        {
            SindicoId = sindicoId,
            TemplateId = template.Id,
            OcorrenciaId = ocorrenciaId,
            MoradorId = morador.Id,
            EmailDestinatario = morador.Pessoa.Email,
            Assunto = assunto,
            CorpoResolvido = corpo,
            ValorMulta = request.ValorMulta,
            EnviadoPorId = enviadoPorId,
            EnviadoEm = DateTime.UtcNow,
            StatusEntrega = enviadoOk ? EmailLogStatus.Delivered : EmailLogStatus.Failed,
            CriadoEm = DateTime.UtcNow
        };

        _db.EmailLogs.Add(log);
        await _db.SaveChangesAsync();

        return new ComunicacaoResponse
        {
            Id = log.Id,
            EmailDestinatario = log.EmailDestinatario,
            Assunto = log.Assunto,
            StatusEntrega = log.StatusEntrega,
            EnviadoEm = log.EnviadoEm
        };
    }

    private static Dictionary<string, string> MontarVariaveisTemplate(
        Ocorrencia ocorrencia, Morador morador, Usuario sindico, decimal? valorMulta, string? prazoResposta)
    {
        var unidadeNum = morador.Unidade?.Numero ?? string.Empty;
        var blocoNome = morador.Unidade?.Bloco?.Nome ?? string.Empty;

        var prazoFormatado = string.Empty;
        if (!string.IsNullOrWhiteSpace(prazoResposta)
            && DateOnly.TryParse(prazoResposta, CultureInfo.InvariantCulture, out var prazoDate))
        {
            prazoFormatado = prazoDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["nome_morador"] = morador.Pessoa.Nome,
            ["unidade"] = unidadeNum,
            ["bloco"] = blocoNome,
            ["condominio"] = ocorrencia.Condominio.Nome,
            ["data_ocorrencia"] = ocorrencia.OcorreuEm.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("pt-BR")),
            ["descricao_ocorrencia"] = ocorrencia.Descricao,
            ["tipo_ocorrencia"] = ocorrencia.TipoOcorrencia,
            ["nome_sindico"] = sindico.Pessoa.Nome,
            ["data_envio"] = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("pt-BR")),
            ["valor_multa"] = valorMulta.HasValue ? valorMulta.Value.ToString("F2", CultureInfo.InvariantCulture) : string.Empty,
            ["prazo_resposta"] = prazoFormatado
        };
    }
}
