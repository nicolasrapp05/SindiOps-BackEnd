using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Helpers;

namespace SindiCore.API.Services.Interfaces;

public interface IMoradorService
{
    Task<PaginatedResponse<MoradorResponse>> GetAllAsync(Guid condominioId, Guid sindicoId, MoradorQueryParams queryParams);
    Task<MoradorDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId);
    Task<MoradorResponse> CreateAsync(CreateMoradorRequest request, Guid sindicoId);
    Task<MoradorResponse> UpdateAsync(Guid id, UpdateMoradorRequest request, Guid sindicoId);
    Task DeleteAsync(Guid id, Guid sindicoId);
    Task<PaginatedResponse<EmailLogResumoResponse>> GetEmailLogsAsync(Guid moradorId, Guid sindicoId, int page, int pageSize);
}
