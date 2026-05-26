using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Helpers;

namespace SindiCore.API.Services.Interfaces;

public interface IManutencaoObrigatoriaService
{
    Task<PaginatedResponse<ManutencaoObrigatoriaResponse>> GetAllAsync(Guid userId, ManutencaoObrigatoriaQueryParams queryParams);
    Task<ManutencaoObrigatoriaResponse> GetByIdAsync(Guid id, Guid userId);
    Task<ManutencaoObrigatoriaResponse> CreateAsync(CreateManutencaoObrigatoriaRequest request, Guid userId);
    Task<ManutencaoObrigatoriaResponse> UpdateAsync(Guid id, CreateManutencaoObrigatoriaRequest request, Guid userId);
    Task<ManutencaoObrigatoriaResponse> RealizarAsync(Guid id, RealizarManutencaoRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
