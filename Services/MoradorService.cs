using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class MoradorService : IMoradorService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;
    private readonly ISupabaseAuthService _supabaseAuth;

    public MoradorService(SindiOpsDbContext db, IMapper mapper, ISupabaseAuthService supabaseAuth)
    {
        _db = db;
        _mapper = mapper;
        _supabaseAuth = supabaseAuth;
    }

    public async Task<PaginatedResponse<MoradorResponse>> GetAllAsync(
        Guid condominioId, Guid sindicoId, MoradorQueryParams q)
    {
        await VerificarCondominioAsync(condominioId, sindicoId);

        var query = QueryDaCarteira(sindicoId)
            .Where(o => o.Unidade.CondominioId == condominioId);

        if (q.BlocoId.HasValue)
            query = query.Where(o => o.Unidade.BlocoId == q.BlocoId.Value);

        if (q.UnidadeId.HasValue)
            query = query.Where(o => o.UnidadeId == q.UnidadeId.Value);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.ToLower();
            query = query.Where(o =>
                o.Morador.Usuario.Nome.ToLower().Contains(search) ||
                o.Morador.Usuario.Email.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var ocupacoes = await query
            .OrderBy(o => o.Morador.Usuario.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<MoradorResponse>
        {
            Data = _mapper.Map<List<MoradorResponse>>(ocupacoes),
            TotalCount = totalCount,
            PageSize = pageSize
        };
    }

    public async Task<MoradorDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId)
    {
        var ocupacao = await QueryDaCarteira(sindicoId)
            .Include(o => o.Morador)
            .ThenInclude(m => m.EmailLogs.OrderByDescending(e => e.EnviadoEm).Take(5))
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException("Morador não encontrado");

        return _mapper.Map<MoradorDetalheResponse>(ocupacao);
    }

    public async Task<MoradorResponse> CreateAsync(CreateMoradorRequest request, Guid sindicoId)
    {
        await VerificarCondominioAsync(request.CondominioId, sindicoId);

        var unidade = await _db.Unidades
            .Include(u => u.Bloco)
            .FirstOrDefaultAsync(u => u.Id == request.UnidadeId && u.CondominioId == request.CondominioId)
            ?? throw new KeyNotFoundException("Unidade não encontrada neste condomínio");

        await GarantirProprietarioUnicoAsync(request.UnidadeId, request.Papel, null);

        var usuario = await ResolverUsuarioDaCarteiraAsync(
            request.Nome, request.Email, request.Telefone, sindicoId);
        var morador = await ObterOuCriarMoradorAsync(usuario);

        var jaOcupa = await _db.MoradorUnidades
            .AnyAsync(o => o.UnidadeId == request.UnidadeId && o.MoradorId == morador.Id);
        if (jaOcupa)
            throw new ValidationException(new[]
            {
                new ValidationFailure("email", "Email já cadastrado nesta unidade")
            });

        var ocupacao = new MoradorUnidade
        {
            Id = Guid.NewGuid(),
            Morador = morador,
            MoradorId = morador.Id,
            UnidadeId = request.UnidadeId,
            Unidade = unidade,
            Papel = request.Papel,
            CriadoEm = DateTime.UtcNow
        };

        _db.MoradorUnidades.Add(ocupacao);
        await _db.SaveChangesAsync();

        return _mapper.Map<MoradorResponse>(ocupacao);
    }

    public async Task<MoradorResponse> UpdateAsync(Guid id, UpdateMoradorRequest request, Guid sindicoId)
    {
        var ocupacao = await QueryDaCarteira(sindicoId)
            .Include(o => o.Morador)
            .ThenInclude(m => m.Usuario)
            .ThenInclude(u => u.Sindico)
            .Include(o => o.Morador)
            .ThenInclude(m => m.Usuario)
            .ThenInclude(u => u.Funcionario)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException("Morador não encontrado");

        var unidade = await _db.Unidades
            .Include(u => u.Bloco)
            .FirstOrDefaultAsync(u => u.Id == request.UnidadeId && u.CondominioId == ocupacao.Unidade.CondominioId)
            ?? throw new KeyNotFoundException("Unidade não encontrada neste condomínio");

        await GarantirProprietarioUnicoAsync(request.UnidadeId, request.Papel, ocupacao.Id);

        var email = request.Email.Trim();
        var emailNorm = email.ToLowerInvariant();
        var usuario = ocupacao.Morador.Usuario;

        if (UsuarioPapeis.TemAcesso(usuario) &&
            !string.Equals(usuario.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("email", "O email de um usuário com acesso não muda pelo cadastro de morador.")
            });
        }

        var outroUsuario = await BuscarUsuarioDaCarteiraAsync(emailNorm, sindicoId);
        if (outroUsuario is not null && outroUsuario.Id != usuario.Id)
        {
            var outroMorador = await ObterOuCriarMoradorAsync(outroUsuario);
            var jaOcupa = await _db.MoradorUnidades
                .AnyAsync(o => o.UnidadeId == request.UnidadeId && o.MoradorId == outroMorador.Id && o.Id != id);
            if (jaOcupa)
                throw new ValidationException(new[]
                {
                    new ValidationFailure("email", "Email já cadastrado nesta unidade")
                });

            ocupacao.MoradorId = outroMorador.Id;
            ocupacao.Morador = outroMorador;
            usuario = outroUsuario;
        }

        if (!UsuarioPapeis.TemAcesso(usuario))
        {
            usuario.Nome = request.Nome.Trim();
            usuario.Email = email;
            usuario.Telefone = TelefoneOuNulo(request.Telefone);
        }
        else if (!string.IsNullOrWhiteSpace(request.Telefone))
        {
            usuario.Telefone = request.Telefone.Trim();
        }

        usuario.AtualizadoEm = DateTime.UtcNow;
        ocupacao.Morador.AtualizadoEm = DateTime.UtcNow;
        ocupacao.UnidadeId = request.UnidadeId;
        ocupacao.Unidade = unidade;
        ocupacao.Papel = request.Papel;
        ocupacao.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        if (UsuarioPapeis.TemAcesso(usuario))
            await _supabaseAuth.SyncUserMetadataAsync(usuario.Id, usuario.Nome, UsuarioPapeis.Cargo(usuario));

        return _mapper.Map<MoradorResponse>(ocupacao);
    }

    public async Task DeleteAsync(Guid id, Guid sindicoId)
    {
        var ocupacao = await QueryDaCarteira(sindicoId)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException("Morador não encontrado");

        ocupacao.DeletadoEm = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<PaginatedResponse<EmailLogResumoResponse>> GetEmailLogsAsync(
        Guid moradorId, Guid sindicoId, int page, int pageSize)
    {
        var ocupacao = await QueryDaCarteira(sindicoId)
            .Select(o => new { o.Id, o.MoradorId })
            .FirstOrDefaultAsync(o => o.Id == moradorId);

        if (ocupacao is null)
            throw new KeyNotFoundException("Morador não encontrado");

        var query = _db.EmailLogs.Where(e => e.MoradorId == ocupacao.MoradorId);
        var totalCount = await query.CountAsync();
        var size = Math.Clamp(pageSize, 1, 100);

        var logs = await query
            .OrderByDescending(e => e.EnviadoEm)
            .Skip((Math.Max(page, 1) - 1) * size)
            .Take(size)
            .Select(e => new EmailLogResumoResponse
            {
                Id = e.Id,
                Assunto = e.Assunto,
                EnviadoEm = e.EnviadoEm,
                StatusEntrega = e.StatusEntrega
            })
            .ToListAsync();

        return new PaginatedResponse<EmailLogResumoResponse>
        {
            Data = logs,
            TotalCount = totalCount,
            PageSize = size
        };
    }

    private IQueryable<MoradorUnidade> QueryDaCarteira(Guid sindicoId) =>
        _db.MoradorUnidades
            .Include(o => o.Morador)
            .ThenInclude(m => m.Usuario)
            .Include(o => o.Unidade)
            .ThenInclude(u => u.Bloco)
            .Where(o => o.Unidade.Condominio.SindicoId == sindicoId);

    private async Task VerificarCondominioAsync(Guid condominioId, Guid sindicoId)
    {
        var pertence = await _db.Condominios
            .AnyAsync(c => c.Id == condominioId && c.SindicoId == sindicoId);

        if (!pertence)
            throw new KeyNotFoundException("Condomínio não encontrado");
    }

    private async Task GarantirProprietarioUnicoAsync(Guid unidadeId, string papel, Guid? ignorarOcupacaoId)
    {
        if (papel != MoradorPapel.Proprietario)
            return;

        var jaTem = await _db.MoradorUnidades.AnyAsync(o =>
            o.UnidadeId == unidadeId &&
            o.Papel == MoradorPapel.Proprietario &&
            o.Id != ignorarOcupacaoId);

        if (jaTem)
            throw new ValidationException(new[]
            {
                new ValidationFailure("papel", "Esta unidade já tem um proprietário.")
            });
    }

    private async Task<Usuario> ResolverUsuarioDaCarteiraAsync(
        string nome, string email, string? telefone, Guid sindicoId)
    {
        var emailNorm = email.Trim().ToLowerInvariant();
        var existente = await BuscarUsuarioDaCarteiraAsync(emailNorm, sindicoId);
        if (existente is not null)
        {
            if (!UsuarioPapeis.TemAcesso(existente))
            {
                existente.Nome = nome.Trim();
                existente.Telefone = TelefoneOuNulo(telefone);
            }
            else if (!string.IsNullOrWhiteSpace(telefone))
            {
                existente.Telefone = telefone.Trim();
            }

            existente.AtualizadoEm = DateTime.UtcNow;
            return existente;
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            Email = email.Trim(),
            Telefone = TelefoneOuNulo(telefone),
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };
        _db.Usuarios.Add(usuario);
        return usuario;
    }

    private Task<Usuario?> BuscarUsuarioDaCarteiraAsync(string emailNorm, Guid sindicoId) =>
        _db.Usuarios
            .Include(u => u.Sindico)
            .Include(u => u.Funcionario)
            .Include(u => u.Morador)
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == emailNorm &&
                ((u.Morador != null && u.Morador.Unidades.Any(o => o.Unidade.Condominio.SindicoId == sindicoId))
                 || (u.Sindico != null && u.Sindico.Id == sindicoId)
                 || (u.Funcionario != null && u.Funcionario.SindicoId == sindicoId)));

    private async Task<Morador> ObterOuCriarMoradorAsync(Usuario usuario)
    {
        if (usuario.Morador is not null)
            return usuario.Morador;

        var existente = usuario.Id == Guid.Empty
            ? null
            : await _db.Moradores.FirstOrDefaultAsync(m => m.UsuarioId == usuario.Id);

        if (existente is not null)
        {
            usuario.Morador = existente;
            return existente;
        }

        var morador = new Morador
        {
            Id = Guid.NewGuid(),
            Usuario = usuario,
            UsuarioId = usuario.Id,
            CriadoEm = DateTime.UtcNow
        };
        usuario.Morador = morador;
        _db.Moradores.Add(morador);
        return morador;
    }

    private static string? TelefoneOuNulo(string? telefone) =>
        string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
}
