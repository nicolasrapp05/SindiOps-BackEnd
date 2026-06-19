using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;

namespace SindiOps.API.Services.Interfaces;

public interface IPerfilService
{
    Task<PerfilResponse> GetMeAsync(Guid userId);
    Task<PerfilResponse> UpdateMeAsync(Guid userId, UpdatePerfilRequest request);
}
