using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Entities;
using SindiCore.API.Infrastructure.Data;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly SindiCoreDbContext _db;
    private readonly IMapper _mapper;

    public EmailTemplateService(SindiCoreDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<EmailTemplateResponse>> GetAllAsync(Guid userId, EmailTemplateQueryParams? queryParams)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var query = _db.EmailTemplates.AsNoTracking().Where(t => t.SindicoId == sindicoId);

        if (!string.IsNullOrWhiteSpace(queryParams?.Tipo))
            query = query.Where(t => t.Tipo == queryParams.Tipo);

        var list = await query.OrderBy(t => t.Nome).ToListAsync();
        return _mapper.Map<List<EmailTemplateResponse>>(list);
    }

    public async Task<EmailTemplateDetalheResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await _db.EmailTemplates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Template não encontrado");

        return _mapper.Map<EmailTemplateDetalheResponse>(entity);
    }

    public async Task<EmailTemplateDetalheResponse> CreateAsync(CreateEmailTemplateRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = new EmailTemplate
        {
            Id = Guid.NewGuid(),
            SindicoId = sindicoId,
            Nome = request.Nome.Trim(),
            Tipo = request.Tipo,
            Assunto = request.Assunto.Trim(),
            Corpo = request.Corpo,
            CriadoEm = DateTime.UtcNow
        };

        _db.EmailTemplates.Add(entity);
        await _db.SaveChangesAsync();

        return _mapper.Map<EmailTemplateDetalheResponse>(entity);
    }

    public async Task<EmailTemplateDetalheResponse> UpdateAsync(Guid id, CreateEmailTemplateRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await _db.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Template não encontrado");

        entity.Nome = request.Nome.Trim();
        entity.Tipo = request.Tipo;
        entity.Assunto = request.Assunto.Trim();
        entity.Corpo = request.Corpo;
        entity.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<EmailTemplateDetalheResponse>(entity);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await _db.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Template não encontrado");

        var temLogs = await _db.EmailLogs.AnyAsync(l => l.TemplateId == id);
        if (temLogs)
            throw new InvalidOperationException(
                "Não é possível excluir este template porque existem registos de envio de e-mail associados.");

        _db.EmailTemplates.Remove(entity);
        await _db.SaveChangesAsync();
    }
}
