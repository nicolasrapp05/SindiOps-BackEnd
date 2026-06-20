using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;

namespace SindiOps.API.Services.Interfaces;

public interface IFuncionarioService
{
    Task<List<FuncionarioResponse>> GetAllAsync(Guid sindicoId, string? cargo, bool? ativo);
    Task<FuncionarioResponse> GetByIdAsync(Guid id, Guid sindicoId);
    Task<FuncionarioResponse> ConvidarAsync(ConvidarFuncionarioRequest request, Guid sindicoId);
    Task<FuncionarioResponse> ReenviarConviteAsync(Guid id, Guid sindicoId);
    Task<FuncionarioResponse> UpdateAsync(Guid id, UpdateFuncionarioRequest request, Guid sindicoId);
    Task<FuncionarioResponse> AtivarAsync(Guid id, Guid sindicoId);
    Task<FuncionarioResponse> DesativarAsync(Guid id, Guid sindicoId, Guid currentUserId);
}
