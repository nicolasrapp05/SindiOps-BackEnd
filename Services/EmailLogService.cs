using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Helpers;
using SindiCore.API.Infrastructure.Data;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Services;

public class EmailLogService : IEmailLogService
{
    private readonly SindiCoreDbContext _db;
    private readonly IMapper _mapper;

    public EmailLogService(SindiCoreDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<EmailLogResponse>> GetAllAsync(Guid userId, EmailLogQueryParams q)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var query = _db.EmailLogs.AsNoTracking()
            .Include(e => e.Morador)
            .Include(e => e.Ocorrencia)
            .Include(e => e.Template)
            .Include(e => e.EnviadoPor)
            .Where(e => e.SindicoId == sindicoId);

        if (q.CondominioId.HasValue)
        {
            var cid = q.CondominioId.Value;
            query = query.Where(e =>
                e.Morador.CondominioId == cid ||
                (e.Ocorrencia != null && e.Ocorrencia.CondominioId == cid));
        }

        if (q.MoradorId.HasValue)
            query = query.Where(e => e.MoradorId == q.MoradorId.Value);

        if (q.OcorrenciaId.HasValue)
            query = query.Where(e => e.OcorrenciaId == q.OcorrenciaId.Value);

        if (!string.IsNullOrWhiteSpace(q.StatusEntrega))
            query = query.Where(e => e.StatusEntrega == q.StatusEntrega);

        if (q.DataInicio.HasValue)
        {
            var start = q.DataInicio.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(e => e.EnviadoEm >= start);
        }

        if (q.DataFim.HasValue)
        {
            var end = q.DataFim.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(e => e.EnviadoEm <= end);
        }

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var items = await query
            .OrderByDescending(e => e.EnviadoEm)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<EmailLogResponse>
        {
            Data = _mapper.Map<List<EmailLogResponse>>(items),
            TotalCount = totalCount,
            PageSize = pageSize
        };
    }

    public async Task<EmailLogDetalheResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await _db.EmailLogs.AsNoTracking()
            .Include(e => e.Morador)
            .Include(e => e.Ocorrencia)
            .Include(e => e.Template)
            .Include(e => e.EnviadoPor)
            .FirstOrDefaultAsync(e => e.Id == id && e.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Registo de e-mail não encontrado");

        return _mapper.Map<EmailLogDetalheResponse>(entity);
    }
}
