using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Helpers;

namespace SindiOps.API.Services.Interfaces;

public interface IMoradorService
{
    Task<PaginatedResponse<MoradorResponse>> GetAllAsync(Guid condominioId, Guid sindicoId, MoradorQueryParams queryParams);
    Task<MoradorDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId);
    Task<MoradorResponse> CreateAsync(CreateMoradorRequest request, Guid sindicoId);
    Task<MoradorResponse> UpdateAsync(Guid id, UpdateMoradorRequest request, Guid sindicoId);
    Task DeleteAsync(Guid id, Guid sindicoId);
    Task<PaginatedResponse<EmailLogResumoResponse>> GetEmailLogsAsync(Guid moradorId, Guid sindicoId, int page, int pageSize);
}
