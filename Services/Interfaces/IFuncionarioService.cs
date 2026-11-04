using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Helpers;

namespace SindiOps.API.Services.Interfaces;

public interface IFuncionarioService
{
    Task<PaginatedResponse<FuncionarioResponse>> GetAllAsync(Guid sindicoId, FuncionarioQueryParams queryParams);
    Task<FuncionarioResponse> GetByIdAsync(Guid id, Guid sindicoId);
    Task<FuncionarioResponse> ConvidarAsync(ConvidarFuncionarioRequest request, Guid sindicoId);
    Task<FuncionarioResponse> ReenviarConviteAsync(Guid id, Guid sindicoId);
    Task<FuncionarioResponse> UpdateAsync(Guid id, UpdateFuncionarioRequest request, Guid sindicoId);
    Task<FuncionarioResponse> AtivarAsync(Guid id, Guid sindicoId);
    Task<FuncionarioResponse> DesativarAsync(Guid id, Guid sindicoId, Guid currentUserId);
    Task DeleteAsync(Guid id, Guid sindicoId, Guid currentUserId);
}
