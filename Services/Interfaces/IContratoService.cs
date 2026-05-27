using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Helpers;

namespace SindiOps.API.Services.Interfaces;

public interface IContratoService
{
    Task<PaginatedResponse<ContratoResponse>> GetAllAsync(Guid sindicoId, ContratoQueryParams queryParams);
    Task<ContratoDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId);
    Task<ContratoDetalheResponse> CreateAsync(CreateContratoRequest request, Guid sindicoId);
    Task<ContratoDetalheResponse> UpdateAsync(Guid id, CreateContratoRequest request, Guid sindicoId);
    Task<ContratoDetalheResponse> UpdateStatusAsync(Guid id, UpdateContratoStatusRequest request, Guid sindicoId);
}
