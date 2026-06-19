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
            .Include(o => o.Morador)!.ThenInclude(m => m!.Unidade)
            .Include(o => o.Morador)!.ThenInclude(m => m!.Bloco)
            .FirstOrDefaultAsync(o => o.Id == ocorrenciaId && o.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Ocorrência não encontrada");

        var template = await _db.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Template não encontrado");

        var morador = await _db.Moradores
            .Include(m => m.Unidade)
            .Include(m => m.Bloco)
            .FirstOrDefaultAsync(m => m.Id == request.MoradorId && m.CondominioId == ocorrencia.CondominioId)
            ?? throw new KeyNotFoundException("Morador não encontrado neste condomínio");

        var sindico = await _db.Sindicos.FirstOrDefaultAsync(s => s.Id == sindicoId)
            ?? throw new KeyNotFoundException("Síndico não encontrado");

        var valores = MontarVariaveisTemplate(ocorrencia, morador, sindico, request.ValorMulta, request.PrazoResposta);

        var assunto = _templateResolver.Resolve(request.AssuntoEditado, valores, nameof(EnviarComunicacaoRequest.AssuntoEditado));
        var corpo = _templateResolver.Resolve(request.CorpoEditado, valores, nameof(EnviarComunicacaoRequest.CorpoEditado));

        Guid? enviadoPorFuncionarioId = null;
        Guid? enviadoPorSindicoId = null;
        if (await UsuarioSindicoScope.IsFuncionarioDoSindicoAsync(_db, enviadoPorId, sindicoId))
            enviadoPorFuncionarioId = enviadoPorId;
        else
            enviadoPorSindicoId = sindicoId;

        var enviadoOk = false;
        try
        {
            enviadoOk = await _emailService.SendAsync(morador.Email, assunto, corpo);
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
            EmailDestinatario = morador.Email,
            Assunto = assunto,
            CorpoResolvido = corpo,
            ValorMulta = request.ValorMulta,
            EnviadoPorFuncionarioId = enviadoPorFuncionarioId,
            EnviadoPorSindicoId = enviadoPorSindicoId,
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
        Ocorrencia ocorrencia, Morador morador, Sindico sindico, decimal? valorMulta, string? prazoResposta)
    {
        var unidadeNum = morador.Unidade?.Numero ?? string.Empty;
        var blocoNome = morador.Bloco?.Nome ?? string.Empty;

        var prazoFormatado = string.Empty;
        if (!string.IsNullOrWhiteSpace(prazoResposta)
            && DateOnly.TryParse(prazoResposta, CultureInfo.InvariantCulture, out var prazoDate))
        {
            prazoFormatado = prazoDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["nome_morador"] = morador.Nome,
            ["unidade"] = unidadeNum,
            ["bloco"] = blocoNome,
            ["condominio"] = ocorrencia.Condominio.Nome,
            ["data_ocorrencia"] = ocorrencia.OcorreuEm.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("pt-BR")),
            ["descricao_ocorrencia"] = ocorrencia.Descricao,
            ["tipo_ocorrencia"] = ocorrencia.TipoOcorrencia,
            ["nome_sindico"] = sindico.Nome,
            ["data_envio"] = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("pt-BR")),
            ["valor_multa"] = valorMulta.HasValue ? valorMulta.Value.ToString("F2", CultureInfo.InvariantCulture) : string.Empty,
            ["prazo_resposta"] = prazoFormatado
        };
    }
}
