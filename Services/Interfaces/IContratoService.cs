using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Helpers;

namespace SindiOps.API.Services.Interfaces;

public interface IContratoService
{
    Task<PaginatedResponse<ContratoResponse>> GetAllAsync(Guid sindicoId, ContratoQueryParams queryParams, Guid userId = default);
    Task<ContratoDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId, Guid userId = default);
    Task<ContratoDetalheResponse> CreateAsync(CreateContratoRequest request, Guid sindicoId, Guid userId = default);
    Task<ContratoDetalheResponse> UpdateAsync(Guid id, CreateContratoRequest request, Guid sindicoId, Guid userId = default);
    Task<ContratoDetalheResponse> UpdateStatusAsync(Guid id, UpdateContratoStatusRequest request, Guid sindicoId, Guid userId = default);
    Task<ContratoDetalheResponse> ReativarAsync(Guid id, Guid sindicoId, Guid userId = default);
}
