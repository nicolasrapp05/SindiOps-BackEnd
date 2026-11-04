using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;

    public EmailTemplateService(SindiOpsDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<EmailTemplateResponse>> GetAllAsync(
        Guid userId, EmailTemplateQueryParams queryParams)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var query = _db.EmailTemplates.AsNoTracking().Where(t => t.SindicoId == sindicoId);

        if (!string.IsNullOrWhiteSpace(queryParams.Tipo))
            query = query.Where(t => t.Tipo == queryParams.Tipo);

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(queryParams.PageSize, 1, 100);
        var page = Math.Max(queryParams.Page, 1);
        var sort = ListSort.Key(queryParams.Sort, "nome", "tipo", "assunto", "atualizadoEm");
        var desc = ListSort.Descending(queryParams.Dir);
        var ordered = sort switch
        {
            "tipo" => ListSort.By(query, t => t.Tipo, desc),
            "assunto" => ListSort.By(query, t => t.Assunto, desc),
            "atualizadoEm" => ListSort.By(query, t => t.AtualizadoEm ?? t.CriadoEm, desc),
            "nome" => ListSort.By(query, t => t.Nome, desc),
            _ => ListSort.By(query, t => t.Nome, false),
        };

        var list = await ordered
            .ThenBy(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<EmailTemplateResponse>
        {
            Data = _mapper.Map<List<EmailTemplateResponse>>(list),
            TotalCount = totalCount,
            PageSize = pageSize
        };
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
