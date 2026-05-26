using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Helpers;

namespace SindiCore.API.Services.Interfaces;

public interface IContratoService
{
    Task<PaginatedResponse<ContratoResponse>> GetAllAsync(Guid sindicoId, ContratoQueryParams queryParams);
    Task<ContratoDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId);
    Task<ContratoDetalheResponse> CreateAsync(CreateContratoRequest request, Guid sindicoId);
    Task<ContratoDetalheResponse> UpdateAsync(Guid id, CreateContratoRequest request, Guid sindicoId);
    Task<ContratoDetalheResponse> UpdateStatusAsync(Guid id, UpdateContratoStatusRequest request, Guid sindicoId);
}
