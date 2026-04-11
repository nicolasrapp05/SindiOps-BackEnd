using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Helpers;

namespace SindiCore.API.Services.Interfaces;

public interface IFornecedorService
{
    Task<PaginatedResponse<FornecedorResponse>> GetAllAsync(Guid sindicoId, FornecedorQueryParams queryParams);
    Task<FornecedorDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId);
    Task<FornecedorDetalheResponse> CreateAsync(CreateFornecedorRequest request, Guid sindicoId);
    Task<FornecedorDetalheResponse> UpdateAsync(Guid id, CreateFornecedorRequest request, Guid sindicoId);
    Task DeleteAsync(Guid id, Guid sindicoId);
}
