using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class CondominioService : ICondominioService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;

    public CondominioService(SindiOpsDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<CondominioResponse>> GetAllAsync(Guid sindicoId, Guid userId)
    {
        var acessiveis = await UsuarioSindicoScope.ObterCondominiosAcessiveisAsync(_db, userId, sindicoId);

        return await _db.Condominios
            .Where(c => c.SindicoId == sindicoId && acessiveis.Contains(c.Id))
            .Select(c => new CondominioResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                EnderecoRua = c.EnderecoRua,
                EnderecoNumero = c.EnderecoNumero,
                EnderecoBairro = c.EnderecoBairro,
                EnderecoCidade = c.EnderecoCidade,
                EnderecoCep = c.EnderecoCep,
                DataEleicao = c.DataEleicao,
                VencimentoMandato = c.VencimentoMandato,
                TotalBlocos = c.Blocos.Count,
                TotalUnidades = c.Unidades.Count,
                CriadoEm = c.CriadoEm
            })
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task<CondominioDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId, Guid userId)
    {
        if (!await UsuarioSindicoScope.FuncionarioPodeAcessarCondominioAsync(_db, userId, sindicoId, id))
            throw new KeyNotFoundException("Condomínio não encontrado");

        var condominio = await _db.Condominios
            .Include(c => c.Blocos)
                .ThenInclude(b => b.Unidades)
            .Include(c => c.Unidades)
            .FirstOrDefaultAsync(c => c.Id == id && c.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Condomínio não encontrado");

        return _mapper.Map<CondominioDetalheResponse>(condominio);
    }

    public async Task<CondominioResponse> CreateAsync(CreateCondominioRequest request, Guid sindicoId)
    {
        var condominio = new Condominio
        {
            SindicoId = sindicoId,
            Nome = request.Nome,
            EnderecoRua = request.EnderecoRua,
            EnderecoNumero = request.EnderecoNumero,
            EnderecoBairro = request.EnderecoBairro,
            EnderecoCidade = request.EnderecoCidade,
            EnderecoCep = request.EnderecoCep,
            DataEleicao = request.DataEleicao,
            VencimentoMandato = request.VencimentoMandato,
            CriadoEm = DateTime.UtcNow
        };

        _db.Condominios.Add(condominio);
        await _db.SaveChangesAsync();

        return new CondominioResponse
        {
            Id = condominio.Id,
            Nome = condominio.Nome,
            EnderecoRua = condominio.EnderecoRua,
            EnderecoNumero = condominio.EnderecoNumero,
            EnderecoBairro = condominio.EnderecoBairro,
            EnderecoCidade = condominio.EnderecoCidade,
            EnderecoCep = condominio.EnderecoCep,
            DataEleicao = condominio.DataEleicao,
            VencimentoMandato = condominio.VencimentoMandato,
            TotalBlocos = 0,
            TotalUnidades = 0,
            CriadoEm = condominio.CriadoEm
        };
    }

    public async Task<CondominioResponse> UpdateAsync(Guid id, CreateCondominioRequest request, Guid sindicoId)
    {
        var condominio = await _db.Condominios
            .FirstOrDefaultAsync(c => c.Id == id && c.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Condomínio não encontrado");

        condominio.Nome = request.Nome;
        condominio.EnderecoRua = request.EnderecoRua;
        condominio.EnderecoNumero = request.EnderecoNumero;
        condominio.EnderecoBairro = request.EnderecoBairro;
        condominio.EnderecoCidade = request.EnderecoCidade;
        condominio.EnderecoCep = request.EnderecoCep;
        condominio.DataEleicao = request.DataEleicao;
        condominio.VencimentoMandato = request.VencimentoMandato;
        condominio.AtualizadoEm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var totalBlocos = await _db.Blocos.CountAsync(b => b.CondominioId == id);
        var totalUnidades = await _db.Unidades.CountAsync(u => u.CondominioId == id);

        return new CondominioResponse
        {
            Id = condominio.Id,
            Nome = condominio.Nome,
            EnderecoRua = condominio.EnderecoRua,
            EnderecoNumero = condominio.EnderecoNumero,
            EnderecoBairro = condominio.EnderecoBairro,
            EnderecoCidade = condominio.EnderecoCidade,
            EnderecoCep = condominio.EnderecoCep,
            DataEleicao = condominio.DataEleicao,
            VencimentoMandato = condominio.VencimentoMandato,
            TotalBlocos = totalBlocos,
            TotalUnidades = totalUnidades,
            CriadoEm = condominio.CriadoEm
        };
    }

    public async Task DeleteAsync(Guid id, Guid sindicoId)
    {
        var condominio = await _db.Condominios
            .FirstOrDefaultAsync(c => c.Id == id && c.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Condomínio não encontrado");

        var hasOcorrencias = await _db.Ocorrencias.AnyAsync(o => o.CondominioId == id);
        var hasContratos = await _db.Contratos.AnyAsync(c => c.CondominioId == id);
        var hasMoradores = await _db.Moradores.AnyAsync(m => m.CondominioId == id);

        if (hasOcorrencias || hasContratos || hasMoradores)
            throw new InvalidOperationException(
                "Não é possível remover o condomínio pois existem registros vinculados (ocorrências, contratos, moradores)");

        _db.Condominios.Remove(condominio);
        await _db.SaveChangesAsync();
    }

    public async Task<List<BlocoResponse>> GetBlocosAsync(Guid condominioId, Guid sindicoId)
    {
        await VerificarPropriedadeAsync(condominioId, sindicoId);

        var blocos = await _db.Blocos
            .Include(b => b.Unidades)
            .Where(b => b.CondominioId == condominioId)
            .OrderBy(b => b.Nome)
            .ToListAsync();

        return _mapper.Map<List<BlocoResponse>>(blocos);
    }

    public async Task<BlocoResponse> CreateBlocoAsync(Guid condominioId, CreateBlocoRequest request, Guid sindicoId)
    {
        await VerificarPropriedadeAsync(condominioId, sindicoId);

        var bloco = new Bloco
        {
            CondominioId = condominioId,
            Nome = request.Nome,
            CriadoEm = DateTime.UtcNow
        };

        _db.Blocos.Add(bloco);
        await _db.SaveChangesAsync();

        return new BlocoResponse { Id = bloco.Id, Nome = bloco.Nome, Unidades = [] };
    }

    public async Task<UnidadeResponse> CreateUnidadeAsync(
        Guid condominioId, Guid blocoId, CreateUnidadeRequest request, Guid sindicoId)
    {
        await VerificarPropriedadeAsync(condominioId, sindicoId);

        var blocoExiste = await _db.Blocos
            .AnyAsync(b => b.Id == blocoId && b.CondominioId == condominioId);

        if (!blocoExiste)
            throw new KeyNotFoundException("Bloco não encontrado neste condomínio");

        var unidade = new Unidade
        {
            BlocoId = blocoId,
            CondominioId = condominioId,
            Numero = request.Numero,
            CriadoEm = DateTime.UtcNow
        };

        _db.Unidades.Add(unidade);
        await _db.SaveChangesAsync();

        return new UnidadeResponse { Id = unidade.Id, Numero = unidade.Numero };
    }

    public async Task DeleteBlocoAsync(Guid condominioId, Guid blocoId, Guid sindicoId)
    {
        await VerificarPropriedadeAsync(condominioId, sindicoId);

        var bloco = await _db.Blocos
            .FirstOrDefaultAsync(b => b.Id == blocoId && b.CondominioId == condominioId)
            ?? throw new KeyNotFoundException("Bloco não encontrado");

        var hasMoradores = await _db.Moradores.AnyAsync(m => m.BlocoId == blocoId);
        if (hasMoradores)
            throw new InvalidOperationException(
                "Não é possível remover o bloco pois existem moradores vinculados às suas unidades");

        _db.Blocos.Remove(bloco);
        await _db.SaveChangesAsync();
    }

    public async Task<BlocoResponse> UpdateBlocoAsync(
        Guid condominioId, Guid blocoId, UpdateBlocoRequest request, Guid sindicoId)
    {
        await VerificarPropriedadeAsync(condominioId, sindicoId);

        var bloco = await _db.Blocos
            .Include(b => b.Unidades)
            .FirstOrDefaultAsync(b => b.Id == blocoId && b.CondominioId == condominioId)
            ?? throw new KeyNotFoundException("Bloco não encontrado");

        bloco.Nome = request.Nome;
        await _db.SaveChangesAsync();

        return _mapper.Map<BlocoResponse>(bloco);
    }

    public async Task<UnidadeResponse> UpdateUnidadeAsync(
        Guid condominioId, Guid blocoId, Guid unidadeId, UpdateUnidadeRequest request, Guid sindicoId)
    {
        await VerificarPropriedadeAsync(condominioId, sindicoId);

        var unidade = await _db.Unidades
            .FirstOrDefaultAsync(u => u.Id == unidadeId && u.BlocoId == blocoId && u.CondominioId == condominioId)
            ?? throw new KeyNotFoundException("Unidade não encontrada");

        unidade.Numero = request.Numero;
        await _db.SaveChangesAsync();

        return new UnidadeResponse { Id = unidade.Id, Numero = unidade.Numero };
    }

    public async Task DeleteUnidadeAsync(
        Guid condominioId, Guid blocoId, Guid unidadeId, Guid sindicoId)
    {
        await VerificarPropriedadeAsync(condominioId, sindicoId);

        var unidade = await _db.Unidades
            .FirstOrDefaultAsync(u => u.Id == unidadeId && u.BlocoId == blocoId && u.CondominioId == condominioId)
            ?? throw new KeyNotFoundException("Unidade não encontrada");

        var hasMoradores = await _db.Moradores.AnyAsync(m => m.UnidadeId == unidadeId);
        if (hasMoradores)
            throw new InvalidOperationException(
                "Não é possível remover a unidade pois existem moradores vinculados a ela");

        _db.Unidades.Remove(unidade);
        await _db.SaveChangesAsync();
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private async Task VerificarPropriedadeAsync(Guid condominioId, Guid sindicoId)
    {
        var pertence = await _db.Condominios
            .AnyAsync(c => c.Id == condominioId && c.SindicoId == sindicoId);

        if (!pertence)
            throw new KeyNotFoundException("Condomínio não encontrado");
    }
}
