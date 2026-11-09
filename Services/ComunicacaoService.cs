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
            .Include(o => o.Morador)!.ThenInclude(m => m!.Usuario)
            .FirstOrDefaultAsync(o => o.Id == ocorrenciaId && o.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Ocorrência não encontrada");

        var template = await _db.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId && t.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Template não encontrado");

        var ocupacao = await _db.MoradorUnidades
            .Include(o => o.Morador).ThenInclude(m => m.Usuario)
            .Include(o => o.Unidade).ThenInclude(u => u.Bloco)
            .FirstOrDefaultAsync(o => o.Id == request.MoradorId && o.Unidade.CondominioId == ocorrencia.CondominioId)
            ?? throw new KeyNotFoundException("Morador não encontrado neste condomínio");

        var sindico = await _db.Sindicos
            .Include(s => s.Usuario)
            .FirstOrDefaultAsync(s => s.Id == sindicoId)
            ?? throw new KeyNotFoundException("Síndico não encontrado");

        var proprietario = await ResolverCopiaProprietarioAsync(request, ocupacao);

        var valores = MontarVariaveisTemplate(ocorrencia, ocupacao, sindico.Usuario, request.ValorMulta, request.PrazoResposta);
        var assunto = _templateResolver.Resolve(request.AssuntoEditado, valores, nameof(EnviarComunicacaoRequest.AssuntoEditado));
        var corpo = _templateResolver.Resolve(request.CorpoEditado, valores, nameof(EnviarComunicacaoRequest.CorpoEditado));

        string? assuntoCopia = null;
        string? corpoCopia = null;
        if (proprietario is not null)
        {
            var valoresCopia = MontarVariaveisTemplate(
                ocorrencia, proprietario, sindico.Usuario, request.ValorMulta, request.PrazoResposta);
            assuntoCopia = _templateResolver.Resolve(
                request.AssuntoEditado, valoresCopia, nameof(EnviarComunicacaoRequest.AssuntoEditado));
            corpoCopia = _templateResolver.Resolve(
                request.CorpoEditado, valoresCopia, nameof(EnviarComunicacaoRequest.CorpoEditado));
        }

        var log = await EnviarERegistrarAsync(
            sindicoId, template.Id, ocorrenciaId, ocupacao, assunto, corpo, request, enviadoPorId);

        EmailLog? logCopia = null;
        if (proprietario is not null && assuntoCopia is not null && corpoCopia is not null)
        {
            logCopia = await EnviarERegistrarAsync(
                sindicoId, template.Id, ocorrenciaId, proprietario, assuntoCopia, corpoCopia, request, enviadoPorId);
        }

        _db.EmailLogs.Add(log);
        if (logCopia is not null)
            _db.EmailLogs.Add(logCopia);

        await _db.SaveChangesAsync();

        var resposta = ParaResposta(log);
        if (logCopia is not null)
            resposta.CopiaProprietario = ParaResposta(logCopia);

        return resposta;
    }

    private async Task<MoradorUnidade?> ResolverCopiaProprietarioAsync(
        EnviarComunicacaoRequest request,
        MoradorUnidade ocupacao)
    {
        if (!request.EnviarCopiaProprietario)
            return null;

        var proprietario = await _db.MoradorUnidades
            .Include(o => o.Morador).ThenInclude(m => m.Usuario)
            .Include(o => o.Unidade).ThenInclude(u => u.Bloco)
            .FirstOrDefaultAsync(o =>
                o.UnidadeId == ocupacao.UnidadeId &&
                o.Papel == MoradorPapel.Proprietario);

        if (proprietario is null)
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    nameof(EnviarComunicacaoRequest.EnviarCopiaProprietario),
                    "Esta unidade não tem um proprietário.")
            });

        if (proprietario.Id == ocupacao.Id
            || proprietario.MoradorId == ocupacao.MoradorId
            || string.Equals(
                proprietario.Morador.Usuario.Email,
                ocupacao.Morador.Usuario.Email,
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return proprietario;
    }

    private async Task<EmailLog> EnviarERegistrarAsync(
        Guid sindicoId,
        Guid templateId,
        Guid ocorrenciaId,
        MoradorUnidade ocupacao,
        string assunto,
        string corpo,
        EnviarComunicacaoRequest request,
        Guid enviadoPorId)
    {
        var enviadoOk = false;
        try
        {
            enviadoOk = await _emailService.SendAsync(ocupacao.Morador.Usuario.Email, assunto, corpo);
        }
        catch
        {
            enviadoOk = false;
        }

        return new EmailLog
        {
            Id = Guid.NewGuid(),
            SindicoId = sindicoId,
            TemplateId = templateId,
            OcorrenciaId = ocorrenciaId,
            MoradorId = ocupacao.MoradorId,
            EmailDestinatario = ocupacao.Morador.Usuario.Email,
            Assunto = assunto,
            CorpoResolvido = corpo,
            ValorMulta = request.ValorMulta,
            EnviadoPorId = enviadoPorId,
            EnviadoEm = DateTime.UtcNow,
            StatusEntrega = enviadoOk ? EmailLogStatus.Delivered : EmailLogStatus.Failed,
            CriadoEm = DateTime.UtcNow
        };
    }

    private static ComunicacaoResponse ParaResposta(EmailLog log) => new()
    {
        Id = log.Id,
        EmailDestinatario = log.EmailDestinatario,
        Assunto = log.Assunto,
        StatusEntrega = log.StatusEntrega,
        EnviadoEm = log.EnviadoEm
    };

    private static Dictionary<string, string> MontarVariaveisTemplate(
        Ocorrencia ocorrencia, MoradorUnidade ocupacao, Usuario sindico, decimal? valorMulta, string? prazoResposta)
    {
        var unidadeNum = ocupacao.Unidade?.Numero ?? string.Empty;
        var blocoNome = ocupacao.Unidade?.Bloco?.Nome ?? string.Empty;

        var prazoFormatado = string.Empty;
        if (!string.IsNullOrWhiteSpace(prazoResposta)
            && DateOnly.TryParse(prazoResposta, CultureInfo.InvariantCulture, out var prazoDate))
        {
            prazoFormatado = prazoDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["nome_morador"] = ocupacao.Morador.Usuario.Nome,
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
