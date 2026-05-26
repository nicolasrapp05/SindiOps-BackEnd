using SindiCore.API.DTOs.Responses;

namespace SindiCore.API.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> GetDashboardAsync(Guid userId, Guid? condominioId);
}
