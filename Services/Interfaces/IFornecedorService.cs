using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Helpers;

namespace SindiOps.API.Services.Interfaces;

public interface IFornecedorService
{
    Task<PaginatedResponse<FornecedorResponse>> GetAllAsync(Guid sindicoId, FornecedorQueryParams queryParams);
    Task<FornecedorDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId);
    Task<FornecedorDetalheResponse> CreateAsync(CreateFornecedorRequest request, Guid sindicoId);
    Task<FornecedorDetalheResponse> UpdateAsync(Guid id, CreateFornecedorRequest request, Guid sindicoId);
    Task DeleteAsync(Guid id, Guid sindicoId);
}
