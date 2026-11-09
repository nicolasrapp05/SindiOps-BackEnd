using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class EmailLogService : IEmailLogService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;

    public EmailLogService(SindiOpsDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<EmailLogListaResponse> GetAllAsync(Guid userId, EmailLogQueryParams q)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var query = _db.EmailLogs.AsNoTracking()
            .Where(e => e.SindicoId == sindicoId);

        if (q.CondominioId.HasValue)
        {
            var cid = q.CondominioId.Value;
            query = query.Where(e =>
                e.Morador.Unidades.Any(u => u.Unidade.CondominioId == cid) ||
                (e.Ocorrencia != null && e.Ocorrencia.CondominioId == cid));
        }

        if (q.MoradorId.HasValue)
        {
            var moradorId = q.MoradorId.Value;
            query = query.Where(e =>
                e.MoradorId == moradorId || e.Morador.Unidades.Any(u => u.Id == moradorId));
        }

        if (q.OcorrenciaId.HasValue)
            query = query.Where(e => e.OcorrenciaId == q.OcorrenciaId.Value);

        if (q.TemplateId.HasValue)
            query = query.Where(e => e.TemplateId == q.TemplateId.Value);

        if (!string.IsNullOrWhiteSpace(q.TemplateTipo))
            query = query.Where(e => e.Template != null && e.Template.Tipo == q.TemplateTipo);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(e =>
                e.Morador.Usuario.Nome.ToLower().Contains(term) ||
                e.EmailDestinatario.ToLower().Contains(term));
        }

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

        var porStatus = await query
            .GroupBy(e => e.StatusEntrega)
            .Select(g => new { Status = g.Key, Quantidade = g.Count() })
            .ToListAsync();

        var resumo = new EmailLogContagemResponse
        {
            TotalEntregues = porStatus.FirstOrDefault(x => x.Status == EmailLogStatus.Delivered)?.Quantidade ?? 0,
            FalhasCriticas = porStatus.FirstOrDefault(x => x.Status == EmailLogStatus.Failed)?.Quantidade ?? 0,
            Total = porStatus.Sum(x => x.Quantidade),
        };

        if (!string.IsNullOrWhiteSpace(q.StatusEntrega))
            query = query.Where(e => e.StatusEntrega == q.StatusEntrega);

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var detalhada = query
            .Include(e => e.Morador).ThenInclude(m => m.Usuario)
            .Include(e => e.Ocorrencia)
            .Include(e => e.Template)
            .Include(e => e.EnviadoPor).ThenInclude(u => u.Sindico)
            .Include(e => e.EnviadoPor).ThenInclude(u => u.Funcionario);

        var sort = ListSort.Key(q.Sort, "template", "enviadoEm", "status");
        var desc = ListSort.Descending(q.Dir);
        var ordered = sort switch
        {
            "template" => ListSort.By(detalhada, e => e.Template != null ? e.Template.Tipo : "", desc),
            "status" => ListSort.By(detalhada, e => e.StatusEntrega, desc),
            "enviadoEm" => ListSort.By(detalhada, e => e.EnviadoEm, desc),
            _ => ListSort.By(detalhada, e => e.EnviadoEm, true),
        };

        var items = await ordered
            .ThenBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new EmailLogListaResponse
        {
            Data = _mapper.Map<List<EmailLogResponse>>(items),
            TotalCount = totalCount,
            PageSize = pageSize,
            Resumo = resumo,
        };
    }

    public async Task<EmailLogDetalheResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await _db.EmailLogs.AsNoTracking()
            .Include(e => e.Morador).ThenInclude(m => m.Usuario)
            .Include(e => e.Ocorrencia)
            .Include(e => e.Template)
            .Include(e => e.EnviadoPor).ThenInclude(u => u.Sindico)
            .Include(e => e.EnviadoPor).ThenInclude(u => u.Funcionario)
            .FirstOrDefaultAsync(e => e.Id == id && e.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Registo de e-mail não encontrado");

        return _mapper.Map<EmailLogDetalheResponse>(entity);
    }
}
