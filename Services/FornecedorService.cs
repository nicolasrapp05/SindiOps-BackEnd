using System.Text.RegularExpressions;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class FornecedorService : IFornecedorService
{
    private readonly SindiOpsDbContext _db;
    private readonly IMapper _mapper;

    public FornecedorService(SindiOpsDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<FornecedorResponse>> GetAllAsync(Guid sindicoId, FornecedorQueryParams q)
    {
        var query = _db.Fornecedores
            .Include(f => f.Servicos)
            .Where(f => f.SindicoId == sindicoId);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.ToLower();
            var searchDigitos = Regex.Replace(search, @"\D", "");

            query = query.Where(f =>
                f.Nome.ToLower().Contains(search) ||
                (f.Cnpj != null && f.Cnpj.Contains(searchDigitos)));
        }

        var totalCount = await query.CountAsync();
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 1);

        var fornecedores = await query
            .OrderBy(f => f.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<FornecedorResponse>
        {
            Data = _mapper.Map<List<FornecedorResponse>>(fornecedores),
            TotalCount = totalCount,
            PageSize = pageSize
        };
    }

    public async Task<FornecedorDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId)
    {
        var fornecedor = await _db.Fornecedores
            .Include(f => f.Servicos)
            .FirstOrDefaultAsync(f => f.Id == id && f.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Fornecedor não encontrado");

        return _mapper.Map<FornecedorDetalheResponse>(fornecedor);
    }

    public async Task<FornecedorDetalheResponse> CreateAsync(CreateFornecedorRequest request, Guid sindicoId)
    {
        await VerificarCnpjDuplicadoAsync(request.Cnpj, sindicoId, excludeId: null);

        var fornecedor = new Fornecedor
        {
            SindicoId = sindicoId,
            Nome = request.Nome,
            Cnpj = NormalizarCnpj(request.Cnpj),
            EnderecoRua = request.EnderecoRua,
            EnderecoNumero = request.EnderecoNumero,
            EnderecoBairro = request.EnderecoBairro,
            EnderecoCidade = request.EnderecoCidade,
            EnderecoCep = request.EnderecoCep,
            Telefone = request.Telefone,
            Email = request.Email,
            Instagram = request.Instagram,
            Website = request.Website,
            NomeContato = request.NomeContato,
            CriadoEm = DateTime.UtcNow
        };

        fornecedor.Servicos = request.Servicos.Select(s => new ServicoFornecedor
        {
            Tipo = s.Tipo,
            Descricao = s.Descricao,
            Quantidade = s.Quantidade,
            CriadoEm = DateTime.UtcNow
        }).ToList();

        _db.Fornecedores.Add(fornecedor);
        await _db.SaveChangesAsync();

        return _mapper.Map<FornecedorDetalheResponse>(fornecedor);
    }

    public async Task<FornecedorDetalheResponse> UpdateAsync(Guid id, CreateFornecedorRequest request, Guid sindicoId)
    {
        var fornecedor = await _db.Fornecedores
            .Include(f => f.Servicos)
            .FirstOrDefaultAsync(f => f.Id == id && f.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Fornecedor não encontrado");

        await VerificarCnpjDuplicadoAsync(request.Cnpj, sindicoId, excludeId: id);

        fornecedor.Nome = request.Nome;
        fornecedor.Cnpj = NormalizarCnpj(request.Cnpj);
        fornecedor.EnderecoRua = request.EnderecoRua;
        fornecedor.EnderecoNumero = request.EnderecoNumero;
        fornecedor.EnderecoBairro = request.EnderecoBairro;
        fornecedor.EnderecoCidade = request.EnderecoCidade;
        fornecedor.EnderecoCep = request.EnderecoCep;
        fornecedor.Telefone = request.Telefone;
        fornecedor.Email = request.Email;
        fornecedor.Instagram = request.Instagram;
        fornecedor.Website = request.Website;
        fornecedor.NomeContato = request.NomeContato;
        fornecedor.AtualizadoEm = DateTime.UtcNow;

        // substitui serviços: remove antigos, insere novos
        _db.ServicosFornecedor.RemoveRange(fornecedor.Servicos);
        fornecedor.Servicos = request.Servicos.Select(s => new ServicoFornecedor
        {
            FornecedorId = fornecedor.Id,
            Tipo = s.Tipo,
            Descricao = s.Descricao,
            Quantidade = s.Quantidade,
            CriadoEm = DateTime.UtcNow
        }).ToList();

        await _db.SaveChangesAsync();

        return _mapper.Map<FornecedorDetalheResponse>(fornecedor);
    }

    public async Task DeleteAsync(Guid id, Guid sindicoId)
    {
        var fornecedor = await _db.Fornecedores
            .FirstOrDefaultAsync(f => f.Id == id && f.SindicoId == sindicoId)
            ?? throw new KeyNotFoundException("Fornecedor não encontrado");

        var temContratos = await _db.Contratos.AnyAsync(c => c.FornecedorId == id);
        if (temContratos)
            throw new InvalidOperationException(
                "Não é possível excluir um fornecedor com contratos vinculados");

        _db.Fornecedores.Remove(fornecedor);
        await _db.SaveChangesAsync();
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task VerificarCnpjDuplicadoAsync(string? cnpj, Guid sindicoId, Guid? excludeId)
    {
        if (string.IsNullOrWhiteSpace(cnpj)) return;

        var cnpjNorm = NormalizarCnpj(cnpj)!;

        var query = _db.Fornecedores.Where(f => f.SindicoId == sindicoId && f.Cnpj == cnpjNorm);

        if (excludeId.HasValue)
            query = query.Where(f => f.Id != excludeId.Value);

        if (await query.AnyAsync())
            throw new ValidationException(new[]
            {
                new ValidationFailure("cnpj", "CNPJ já cadastrado para este síndico")
            });
    }

    private static string? NormalizarCnpj(string? cnpj) =>
        string.IsNullOrWhiteSpace(cnpj) ? null : Regex.Replace(cnpj, @"\D", "");
}
