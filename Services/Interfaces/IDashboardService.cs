using SindiOps.API.DTOs.Responses;

namespace SindiOps.API.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> GetDashboardAsync(Guid userId, Guid? condominioId);
}
