using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Infrastructure.Storage;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class OcorrenciaService : IOcorrenciaService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;
    private readonly IStorageService _storage;

    public OcorrenciaService(
        SindiOpsDbContext db,
        IMapper mapper,
        IStorageService storage)
    {
        _db = db;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<PaginatedResponse<OcorrenciaResponse>> GetAllAsync(Guid userId, OcorrenciaQueryParams q)
    {
        if (q.CondominioId == Guid.Empty)
            throw new ValidationException(new[]
            {
                new ValidationFailure("condominioId", "condominioId é obrigatório")
            });

        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var condominioOk = await _db.Condominios
            .AnyAsync(c => c.Id == q.CondominioId && c.SindicoId == sindicoId);
        if (!condominioOk)
            throw new KeyNotFoundException("Condomínio não encontrado");

        var query = _db.Ocorrencias
            .Include(o => o.Morador)!.ThenInclude(m => m!.Unidade)
            .Include(o => o.Bloco)
            .Include(o => o.Unidade)
            .Include(o => o.RegistradoPorFuncionario)
            .Include(o => o.RegistradoPorSindico)
            .Include(o => o.Midias)
            .Where(o => o.CondominioId == q.CondominioId);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(o =>
                o.Descricao.ToLower().Contains(term) ||
                o.TipoOcorrencia.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(o => o.Status == q.Status);

        if (!string.IsNullOrWhiteSpace(q.Origem))
            query = query.Where(o => o.Origem == q.Origem);

        if (!string.IsNullOrWhiteSpace(q.TipoOcorrencia))
            query = query.Where(o => o.TipoOcorrencia == q.TipoOcorrencia);

        if (q.DataInicio.HasValue)
        {
            var start = q.DataInicio.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(o => o.OcorreuEm >= start);
        }

        if (q.DataFim.HasValue)
        {
            var end = q.DataFim.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(o => o.OcorreuEm <= end);
        }

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var items = await query
            .OrderByDescending(o => o.OcorreuEm)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<OcorrenciaResponse>
        {
            Data = _mapper.Map<List<OcorrenciaResponse>>(items),
            TotalCount = totalCount,
            PageSize = pageSize
        };
    }

    public async Task<OcorrenciaDetalheResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await _db.Ocorrencias
            .Include(o => o.Morador)!.ThenInclude(m => m!.Unidade)
            .Include(o => o.Bloco)
            .Include(o => o.Unidade)
            .Include(o => o.RegistradoPorFuncionario)
            .Include(o => o.RegistradoPorSindico)
            .Include(o => o.Midias)
            .Include(o => o.EmailLogs)
            .FirstOrDefaultAsync(o => o.Id == id && o.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Ocorrência não encontrada");

        var det = _mapper.Map<OcorrenciaDetalheResponse>(entity);

        const int exp = 3600;
        var expira = DateTime.UtcNow.AddSeconds(exp);
        det.Midias = [];
        foreach (var m in entity.Midias.OrderBy(x => x.CriadoEm))
        {
            var signed = await _storage.GetSignedUrlAsync(m.UrlArquivo, exp);
            det.Midias.Add(new MidiaResponse
            {
                Id = m.Id,
                SignedUrl = signed,
                TipoArquivo = m.TipoArquivo,
                ExpiresAt = expira
            });
        }

        return det;
    }

    public async Task<OcorrenciaResponse> CreateAsync(CreateOcorrenciaRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        if (!await UsuarioSindicoScope.IsFuncionarioOuSindicoPrincipalAsync(_db, userId, sindicoId))
            throw new ValidationException(new[]
            {
                new ValidationFailure("", "Utilizador não autorizado a registrar ocorrências neste condomínio")
            });

        await ValidarCondominioAsync(request.CondominioId, sindicoId);
        await ValidarReferenciasAsync(request.CondominioId, request.MoradorId, request.BlocoId, request.UnidadeId);

        var ocorreuEmUtc = ToPostgreTimestampUtc(request.OcorreuEm);

        Guid? registradoFuncionarioId = null;
        Guid? registradoSindicoId = null;
        if (await UsuarioSindicoScope.IsFuncionarioDoSindicoAsync(_db, userId, sindicoId))
            registradoFuncionarioId = userId;
        else
            registradoSindicoId = sindicoId;

        var entity = new Ocorrencia
        {
            CondominioId = request.CondominioId,
            RegistradoPorFuncionarioId = registradoFuncionarioId,
            RegistradoPorSindicoId = registradoSindicoId,
            MoradorId = request.MoradorId,
            Origem = request.Origem,
            TipoLocal = request.TipoLocal,
            BlocoId = request.BlocoId,
            UnidadeId = request.UnidadeId,
            TipoOcorrencia = request.TipoOcorrencia,
            Descricao = request.Descricao,
            OcorreuEm = ocorreuEmUtc,
            Status = OcorrenciaStatus.Nova,
            CriadoEm = DateTime.UtcNow
        };

        _db.Ocorrencias.Add(entity);
        await _db.SaveChangesAsync();

        await RecarregarParaListagemAsync(entity);

        return _mapper.Map<OcorrenciaResponse>(entity);
    }

    public async Task<OcorrenciaResponse> UpdateAsync(Guid id, CreateOcorrenciaRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        if (!await UsuarioSindicoScope.IsFuncionarioOuSindicoPrincipalAsync(_db, userId, sindicoId))
            throw new ValidationException(new[]
            {
                new ValidationFailure("", "Utilizador não autorizado a atualizar ocorrências neste condomínio")
            });

        var entity = await _db.Ocorrencias
            .Include(o => o.Midias)
            .FirstOrDefaultAsync(o => o.Id == id && o.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Ocorrência não encontrada");

        if (entity.CondominioId != request.CondominioId)
            await ValidarCondominioAsync(request.CondominioId, sindicoId);

        await ValidarReferenciasAsync(request.CondominioId, request.MoradorId, request.BlocoId, request.UnidadeId);

        entity.CondominioId = request.CondominioId;
        entity.MoradorId = request.MoradorId;
        entity.Origem = request.Origem;
        entity.TipoLocal = request.TipoLocal;
        entity.BlocoId = request.BlocoId;
        entity.UnidadeId = request.UnidadeId;
        entity.TipoOcorrencia = request.TipoOcorrencia;
        entity.Descricao = request.Descricao;
        entity.OcorreuEm = ToPostgreTimestampUtc(request.OcorreuEm);
        entity.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await RecarregarParaListagemAsync(entity);

        return _mapper.Map<OcorrenciaResponse>(entity);
    }

    public async Task<OcorrenciaResponse> UpdateStatusAsync(Guid id, UpdateOcorrenciaStatusRequest request, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var entity = await _db.Ocorrencias
            .Include(o => o.Midias)
            .Include(o => o.Morador)!.ThenInclude(m => m!.Unidade)
            .Include(o => o.Bloco)
            .Include(o => o.Unidade)
            .Include(o => o.RegistradoPorFuncionario)
            .Include(o => o.RegistradoPorSindico)
            .FirstOrDefaultAsync(o => o.Id == id && o.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Ocorrência não encontrada");

        if (!TransicaoStatusPermitida(entity.Status, request.Status))
            throw new ValidationException(new[]
            {
                new ValidationFailure("status", $"Transição de status inválida: {entity.Status} → {request.Status}")
            });

        entity.Status = request.Status;
        entity.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<OcorrenciaResponse>(entity);
    }

    public async Task<MidiaResponse> UploadMidiaAsync(
        Guid ocorrenciaId, IFormFile arquivo, string tipo, Guid userId)
    {
        if (arquivo is null || arquivo.Length == 0)
            throw new ValidationException(new[]
            {
                new ValidationFailure("arquivo", "Arquivo é obrigatório")
            });

        var tipoNorm = tipo.Trim().ToLowerInvariant();
        if (tipoNorm is not ("image" or "video"))
            throw new ValidationException(new[]
            {
                new ValidationFailure("tipo", "Tipo deve ser 'image' ou 'video'")
            });

        var ct = arquivo.ContentType.ToLowerInvariant();
        if (tipoNorm == "image" && !ct.StartsWith("image/", StringComparison.Ordinal))
            throw new ValidationException(new[]
            {
                new ValidationFailure("arquivo", "Arquivo não corresponde ao tipo 'image'")
            });
        if (tipoNorm == "video" && !ct.StartsWith("video/", StringComparison.Ordinal))
            throw new ValidationException(new[]
            {
                new ValidationFailure("arquivo", "Arquivo não corresponde ao tipo 'video'")
            });

        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        if (!await UsuarioSindicoScope.IsFuncionarioOuSindicoPrincipalAsync(_db, userId, sindicoId))
            throw new ValidationException(new[]
            {
                new ValidationFailure("", "Utilizador não autorizado a enviar mídias neste condomínio")
            });

        Guid? enviadoFuncionarioId = null;
        Guid? enviadoSindicoId = null;
        if (await UsuarioSindicoScope.IsFuncionarioDoSindicoAsync(_db, userId, sindicoId))
            enviadoFuncionarioId = userId;
        else
            enviadoSindicoId = sindicoId;

        var ocorrencia = await _db.Ocorrencias
            .FirstOrDefaultAsync(o => o.Id == ocorrenciaId && o.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Ocorrência não encontrada");

        var filePath = StorageFileNameHelper.Build(ocorrencia.CondominioId, ocorrenciaId, arquivo.FileName);

        await using (var stream = arquivo.OpenReadStream())
        {
            await _storage.UploadAsync(stream, filePath, arquivo.ContentType);
        }

        var midia = new MidiaOcorrencia
        {
            OcorrenciaId = ocorrenciaId,
            UrlArquivo = filePath,
            TipoArquivo = tipoNorm,
            EnviadoPorFuncionarioId = enviadoFuncionarioId,
            EnviadoPorSindicoId = enviadoSindicoId,
            CriadoEm = DateTime.UtcNow
        };

        _db.MidiasOcorrencia.Add(midia);
        await _db.SaveChangesAsync();

        const int exp = 3600;
        var signed = await _storage.GetSignedUrlAsync(filePath, exp);

        return new MidiaResponse
        {
            Id = midia.Id,
            SignedUrl = signed,
            TipoArquivo = midia.TipoArquivo,
            ExpiresAt = DateTime.UtcNow.AddSeconds(exp)
        };
    }

    public async Task DeleteMidiaAsync(Guid ocorrenciaId, Guid midiaId, Guid userId)
    {
        var sindicoId = await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);

        var midia = await _db.MidiasOcorrencia
            .Include(m => m.Ocorrencia)
            .FirstOrDefaultAsync(m =>
                m.Id == midiaId &&
                m.OcorrenciaId == ocorrenciaId &&
                m.Ocorrencia.Condominio.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Mídia não encontrada");

        await _storage.DeleteAsync(midia.UrlArquivo);

        _db.MidiasOcorrencia.Remove(midia);
        await _db.SaveChangesAsync();
    }

    private async Task RecarregarParaListagemAsync(Ocorrencia entity)
    {
        await _db.Entry(entity).Collection(o => o.Midias).LoadAsync();
        await _db.Entry(entity).Reference(o => o.Morador).LoadAsync();
        if (entity.Morador != null)
        {
            await _db.Entry(entity.Morador).Reference(m => m.Unidade).LoadAsync();
        }

        await _db.Entry(entity).Reference(o => o.Bloco).LoadAsync();
        await _db.Entry(entity).Reference(o => o.Unidade).LoadAsync();
        await _db.Entry(entity).Reference(o => o.RegistradoPorFuncionario).LoadAsync();
        await _db.Entry(entity).Reference(o => o.RegistradoPorSindico).LoadAsync();
    }

    private async Task ValidarCondominioAsync(Guid condominioId, Guid sindicoId)
    {
        var ok = await _db.Condominios.AnyAsync(c => c.Id == condominioId && c.SindicoId == sindicoId);
        if (!ok)
            throw new KeyNotFoundException("Condomínio não encontrado");
    }

    private async Task ValidarReferenciasAsync(
        Guid condominioId, Guid? moradorId, Guid? blocoId, Guid? unidadeId)
    {
        if (moradorId.HasValue)
        {
            var ok = await _db.Moradores.AnyAsync(m =>
                m.Id == moradorId && m.CondominioId == condominioId);
            if (!ok)
                throw new KeyNotFoundException("Morador inválido para este condomínio");
        }

        if (blocoId.HasValue)
        {
            var ok = await _db.Blocos.AnyAsync(b => b.Id == blocoId && b.CondominioId == condominioId);
            if (!ok)
                throw new KeyNotFoundException("Bloco inválido para este condomínio");
        }

        if (unidadeId.HasValue)
        {
            var ok = await _db.Unidades.AnyAsync(u => u.Id == unidadeId && u.CondominioId == condominioId);
            if (!ok)
                throw new KeyNotFoundException("Unidade inválida para este condomínio");
        }
    }

    private static bool TransicaoStatusPermitida(string atual, string novo)
    {
        if (atual == novo)
            return true;

        return (atual, novo) switch
        {
            (OcorrenciaStatus.Nova, OcorrenciaStatus.EmAndamento) => true,
            (OcorrenciaStatus.Nova, OcorrenciaStatus.Cancelada) => true,
            (OcorrenciaStatus.Cancelada, OcorrenciaStatus.EmAndamento) => true,
            (OcorrenciaStatus.EmAndamento, OcorrenciaStatus.Finalizada) => true,
            (OcorrenciaStatus.EmAndamento, OcorrenciaStatus.Cancelada) => true,
            _ => false
        };
    }

    /// <summary>PostgreSQL <c>timestamptz</c> exige <see cref="DateTimeKind.Utc"/>; JSON costuma devolver <see cref="DateTimeKind.Unspecified"/>.</summary>
    private static DateTime ToPostgreTimestampUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
            return value;
        if (value.Kind == DateTimeKind.Local)
            return value.ToUniversalTime();
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
