using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using SindiCore.API.Constants;
using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Entities;
using SindiCore.API.Infrastructure.Data;
using SindiCore.API.Infrastructure.Email;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Services;

public class ComunicacaoService : IComunicacaoService
{
    private readonly SindiCoreDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ITemplateResolver _templateResolver;

    public ComunicacaoService(
        SindiCoreDbContext db,
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

        if (!await UsuarioSindicoScope.IsFuncionarioDoSindicoAsync(_db, enviadoPorId, sindicoId))
            throw new ValidationException(new[]
            {
                new ValidationFailure("", "Apenas funcionários podem enviar comunicações")
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

        var valores = MontarVariaveisTemplate(ocorrencia, morador, sindico, request.ValorMulta);

        var assunto = _templateResolver.Resolve(request.AssuntoEditado, valores, nameof(EnviarComunicacaoRequest.AssuntoEditado));
        var corpo = _templateResolver.Resolve(request.CorpoEditado, valores, nameof(EnviarComunicacaoRequest.CorpoEditado));

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
            EnviadoPorId = enviadoPorId,
            EnviadoEm = DateTime.UtcNow,
            StatusEntrega = enviadoOk ? EmailLogStatus.Sent : EmailLogStatus.Failed,
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
        Ocorrencia ocorrencia, Morador morador, Sindico sindico, decimal? valorMulta)
    {
        var unidadeNum = morador.Unidade?.Numero ?? string.Empty;
        var blocoNome = morador.Bloco?.Nome ?? string.Empty;

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["nome_morador"] = morador.Nome,
            ["unidade"] = unidadeNum,
            ["bloco"] = blocoNome,
            ["condominio"] = ocorrencia.Condominio.Nome,
            ["data_ocorrencia"] = ocorrencia.OcorreuEm.ToString("dd/MM/yyyy HH:mm") + " UTC",
            ["descricao_ocorrencia"] = ocorrencia.Descricao,
            ["tipo_ocorrencia"] = ocorrencia.TipoOcorrencia,
            ["nome_sindico"] = sindico.Nome,
            ["data_envio"] = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm") + " UTC",
            ["valor_multa"] = valorMulta.HasValue ? valorMulta.Value.ToString("F2") : string.Empty,
            ["prazo_resposta"] = string.Empty
        };
    }
}
