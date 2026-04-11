using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Helpers;

namespace SindiCore.API.Services.Interfaces;

public interface ISolicitacaoManutencaoService
{
    Task<PaginatedResponse<SolicitacaoManutencaoResponse>> GetAllAsync(Guid userId, SolicitacaoManutencaoQueryParams queryParams);
    Task<SolicitacaoManutencaoResponse> GetByIdAsync(Guid id, Guid userId);
    Task<SolicitacaoManutencaoResponse> CreateAsync(CreateSolicitacaoManutencaoRequest request, Guid userId);
    Task<SolicitacaoManutencaoResponse> UpdateAsync(Guid id, CreateSolicitacaoManutencaoRequest request, Guid userId);
    Task<SolicitacaoManutencaoResponse> UpdateStatusAsync(Guid id, UpdateSolicitacaoStatusRequest request, Guid userId);
}
