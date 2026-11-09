using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Helpers;

namespace SindiOps.API.Services.Interfaces;

public interface IMoradorService
{
    Task<PaginatedResponse<MoradorResponse>> GetAllAsync(Guid? condominioId, Guid sindicoId, MoradorQueryParams queryParams, Guid userId = default);
    Task<MoradorDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId, Guid userId = default);
    Task<MoradorResponse> CreateAsync(CreateMoradorRequest request, Guid sindicoId, Guid userId = default);
    Task<MoradorResponse> UpdateAsync(Guid id, UpdateMoradorRequest request, Guid sindicoId, Guid userId = default);
    Task DeleteAsync(Guid id, Guid sindicoId, Guid userId = default);
    Task<PaginatedResponse<EmailLogResumoResponse>> GetEmailLogsAsync(Guid moradorId, Guid sindicoId, int page, int pageSize, Guid userId = default);
}
